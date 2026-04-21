using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ServiceTicket.Core.Interfaces.Messaging;

namespace ServiceTicket.Infrastructure.Messaging;

public class RabbitMQPublisher : IMessagePublisher
{
    private readonly RabbitMQConnection _connection;
    private readonly ILogger<RabbitMQPublisher> _logger;

    public RabbitMQPublisher(RabbitMQConnection connection, ILogger<RabbitMQPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            using var channel = await _connection.CreateChannelAsync();

            var jsonMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: routingKey,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Mensagem publicada na fila {RoutingKey}: {Message}", routingKey, jsonMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagem na fila {RoutingKey}", routingKey);
            throw;
        }
    }
}
