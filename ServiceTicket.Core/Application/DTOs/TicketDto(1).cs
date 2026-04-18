// Application/DTOs/TicketDto.cs
namespace ServiceTicket.Core.Application.DTOs;

public record TicketDto(
    Guid Id,
    string ClientName,
    string ProblemDescription,
    string Priority,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateTicketDto(
    string ClientName,
    string ProblemDescription,
    Priority Priority
);

public record UpdateTicketStatusDto(
    TicketStatus NewStatus
);