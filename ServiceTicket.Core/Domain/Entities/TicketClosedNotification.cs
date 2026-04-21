// Domain/Entities/TicketClosedNotification.cs
namespace ServiceTicket.Core.Domain.Entities;

public class TicketClosedNotification
{
    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public string Message { get; private set; }
    public DateTime NotifiedAt { get; private set; }
    public string CorrelationId { get; private set; }

    public TicketClosedNotification(Guid ticketId, string message, string? correlationId = null)
    {
        Id = Guid.NewGuid();
        TicketId = ticketId;
        Message = message;
        NotifiedAt = DateTime.UtcNow;
        CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? Guid.NewGuid().ToString("N") : correlationId;
    }
}