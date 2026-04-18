namespace ServiceTicket.Core.Application.DTOs
{
    public record TicketDto
    {
        Guid Id,
        string ClientName,
        string ProblemDescription,
        string Priority,
        string Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    };

    public record CreateTicketDto
    {
        string ClientName,
        string ProblemDescription,
        string Priority
    };

    public record UpdateTicketStatusDto
    {
        string? ClientName,
        string? ProblemDescription,
        string? Priority,
        string? Status
    };

}