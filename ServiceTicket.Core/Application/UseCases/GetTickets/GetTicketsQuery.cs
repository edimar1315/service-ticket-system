namespace ServiceTicket.Core.Application.UseCases.GetTickets;

public record GetTicketsQuery(
    string? ClientName,
    string? Status,
    string? Priority,
    int PageNumber = 1,
    int PageSize = 10);