namespace ServiceTicket.Core.Events;

public class TicketFinalizedEvent
{
    public Guid TicketId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime FinalizedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
