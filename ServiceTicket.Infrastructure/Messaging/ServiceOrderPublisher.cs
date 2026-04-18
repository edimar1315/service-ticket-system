// Messaging/ServiceOrderPublisher.cs
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ServiceTicket.Core.Domain.Interfaces;
using System.Text;

namespace ServiceTicket.Infrastructure.Messaging;

public class ServiceOrderPublisher : IServiceOrderPublisher
{
    private readonly RabbitMQConnection _connection;
    private readonly ILogger<ServiceOrderPublisher> _logger;

    public ServiceOrderPublisher(
        RabbitMQConnection connection,
        ILogger<ServiceOrderPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task PublishAsync(Guid ticketId)
    {
        await using var channel = await _connection.CreateChannelAsync();

        var body = Encoding.UTF8.GetBytes(ticketId.ToString());

        var properties = new BasicProperties
        {
            Persistent = true // mensagem sobrevive a restart
        };

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: RabbitMQConnection.FinishedOrdersQueue,
            mandatory: false,
            basicProperties: properties,
            body: body);

        _logger.LogInformation("Ticket {TicketId} publicado na fila.", ticketId);
    }
}