namespace ServiceTicket.Core.Application.DTOs;

/// <summary>
/// DTO contendo informações de um analista e seus tickets
/// </summary>
public record AnalystDto(
    Guid Id,
    string FullName,
    string Email,
    int OpenTickets,
    int InProgressTickets,
    int TotalTickets
);

/// <summary>
/// DTO com lista de tickets por analista
/// </summary>
public record TicketsByAnalystDto(
    Guid AnalystId,
    string AnalystName,
    string AnalystEmail,
    int TotalTickets,
    int OpenCount,
    int InProgressCount,
    int FinishedCount,
    IEnumerable<TicketSummaryDto> Tickets
);

/// <summary>
/// Resumo simplificado de um ticket
/// </summary>
public record TicketSummaryDto(
    Guid Id,
    string ClientName,
    string Priority,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
