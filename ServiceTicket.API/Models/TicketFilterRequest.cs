namespace ServiceTicket.API.Models;

public class TicketFilterRequest
{
    public int? Status { get; set; }
    public int? Priority { get; set; }
    public string? ClientName { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}