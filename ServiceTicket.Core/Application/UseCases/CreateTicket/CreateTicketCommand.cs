// UseCases/CreateTicket/CreateTicketCommand.cs
namespace ServiceTicket.Core.Application.UseCases.CreateTicket;

public record CreateTicketCommand(
    string? ClientName,
    string? ProblemDescription,
    string? Priority
    );