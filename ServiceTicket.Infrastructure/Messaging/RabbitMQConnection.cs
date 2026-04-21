// Messaging/RabbitMQConnection.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ServiceTicket.Infrastructure.Messaging;

public class RabbitMQConnection : IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMQConnection> _logger;

    public const string FinishedOrdersQueue = "finished-orders";
    public const string FinishedOrdersDeadLetterQueue = "finished-orders.dlq";

    public RabbitMQConnection(IConfiguration configuration, ILogger<RabbitMQConnection> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _logger.LogInformation("RabbitMQ conectado com sucesso.");
    }

    public async Task<IChannel> CreateChannelAsync()
    {
        var channel = await _connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: FinishedOrdersQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.QueueDeclareAsync(
            queue: FinishedOrdersDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        return channel;
    }

    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
}