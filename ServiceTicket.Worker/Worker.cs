using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServiceTicket.Core.Domain.Entities;
using ServiceTicket.Core.Domain.Interfaces;
using ServiceTicket.Core.Events;
using ServiceTicket.Infrastructure.Messaging;

namespace ServiceTicket.Worker;

public class Worker : BackgroundService
{
    private const int MaxRetryAttempts = 3;
    private const string RetryCountHeader = "x-retry-count";
    private const string CorrelationIdHeader = "x-correlation-id";

    private readonly ILogger<Worker> _logger;
    private readonly RabbitMQConnection _rabbitMqConnection;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public Worker(
        ILogger<Worker> logger,
        RabbitMQConnection rabbitMqConnection,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _rabbitMqConnection = rabbitMqConnection;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var channel = await _rabbitMqConnection.CreateChannelAsync();
        await channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var correlationId = GetOrCreateCorrelationId(ea.BasicProperties);
            using var logScope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            });

            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var ticketFinalizedEvent = JsonSerializer.Deserialize<TicketFinalizedEvent>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (ticketFinalizedEvent is null)
                {
                    throw new InvalidOperationException("Mensagem inválida para TicketFinalizedEvent.");
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                var notificationMessage = $"Ticket {ticketFinalizedEvent.TicketId} finalizado para cliente {ticketFinalizedEvent.ClientName} em {ticketFinalizedEvent.FinalizedAt:O} com status {ticketFinalizedEvent.Status}.";
                var notification = new TicketClosedNotification(ticketFinalizedEvent.TicketId, notificationMessage, correlationId);

                await notificationRepository.AddAsync(notification);

                _logger.LogInformation(
                    "Notificação persistida para ticket finalizado. TicketId: {TicketId}, CorrelationId: {CorrelationId}",
                    ticketFinalizedEvent.TicketId,
                    correlationId);

                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                var retryCount = GetRetryCount(ea.BasicProperties);

                if (retryCount < MaxRetryAttempts)
                {
                    await PublishRetryAsync(channel, ea, retryCount + 1, correlationId, stoppingToken);
                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);

                    _logger.LogWarning(
                        ex,
                        "Falha ao processar mensagem. Reenfileirada para tentativa {RetryAttempt}/{MaxRetryAttempts}. CorrelationId: {CorrelationId}",
                        retryCount + 1,
                        MaxRetryAttempts,
                        correlationId);

                    return;
                }

                await PublishToDeadLetterAsync(channel, ea, correlationId, stoppingToken);
                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);

                _logger.LogError(
                    ex,
                    "Falha ao processar mensagem após {MaxRetryAttempts} tentativas. Enviada para DLQ {DeadLetterQueue}. CorrelationId: {CorrelationId}",
                    MaxRetryAttempts,
                    RabbitMQConnection.FinishedOrdersDeadLetterQueue,
                    correlationId);
            }
        };

        var consumerTag = await channel.BasicConsumeAsync(
            queue: RabbitMQConnection.FinishedOrdersQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Worker consumindo fila {Queue} com consumerTag {ConsumerTag}", RabbitMQConnection.FinishedOrdersQueue, consumerTag);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
        }
    }

    private static int GetRetryCount(IReadOnlyBasicProperties? properties)
    {
        if (properties?.Headers is null || !properties.Headers.TryGetValue(RetryCountHeader, out var rawValue) || rawValue is null)
        {
            return 0;
        }

        return rawValue switch
        {
            byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out var value) => value,
            int value => value,
            long value => (int)value,
            _ => 0
        };
    }

    private static async Task PublishRetryAsync(IChannel channel, BasicDeliverEventArgs ea, int retryCount, string correlationId, CancellationToken cancellationToken)
    {
        var properties = new BasicProperties
        {
            Persistent = true,
            CorrelationId = correlationId,
            Headers = ea.BasicProperties?.Headers is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?>(ea.BasicProperties.Headers)
        };

        properties.Headers[RetryCountHeader] = Encoding.UTF8.GetBytes(retryCount.ToString());
        properties.Headers[CorrelationIdHeader] = Encoding.UTF8.GetBytes(correlationId);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: RabbitMQConnection.FinishedOrdersQueue,
            mandatory: false,
            basicProperties: properties,
            body: ea.Body,
            cancellationToken: cancellationToken);
    }

    private static async Task PublishToDeadLetterAsync(IChannel channel, BasicDeliverEventArgs ea, string correlationId, CancellationToken cancellationToken)
    {
        var properties = new BasicProperties
        {
            Persistent = true,
            CorrelationId = correlationId,
            Headers = ea.BasicProperties?.Headers is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?>(ea.BasicProperties.Headers)
        };

        properties.Headers[CorrelationIdHeader] = Encoding.UTF8.GetBytes(correlationId);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: RabbitMQConnection.FinishedOrdersDeadLetterQueue,
            mandatory: false,
            basicProperties: properties,
            body: ea.Body,
            cancellationToken: cancellationToken);
    }

    private static string GetOrCreateCorrelationId(IReadOnlyBasicProperties? properties)
    {
        if (!string.IsNullOrWhiteSpace(properties?.CorrelationId))
        {
            return properties.CorrelationId!;
        }

        if (properties?.Headers is not null && properties.Headers.TryGetValue(CorrelationIdHeader, out var rawValue) && rawValue is not null)
        {
            if (rawValue is byte[] bytes)
            {
                var value = Encoding.UTF8.GetString(bytes);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
            else if (rawValue is string text && !string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        return Guid.NewGuid().ToString("N");
    }
}
