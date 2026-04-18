namespace ServiceTicket.Core.Application.UseCases.CreateTicket;

public record CreateTicketResponse(
    Guid Id,
    string ClientName,
    string Status,
    DateTime CreatedAt);