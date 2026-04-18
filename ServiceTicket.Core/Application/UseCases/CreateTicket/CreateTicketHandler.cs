// UseCases/CreateTicket/CreateTicketHandler.cs
using ServiceTicket.Core.Domain.Entities;
using ServiceTicket.Core.Domain.Enums;
using ServiceTicket.Core.Domain.Interfaces;

namespace ServiceTicket.Core.Application.UseCases.CreateTicket;

public class CreateTicketHandler
{
    private readonly ITicketRepository _ticketRepository;

    public CreateTicketHandler(ITicketRepository ticketRepository)
    {
        ArgumentNullException.ThrowIfNull(ticketRepository);
        _ticketRepository = ticketRepository;
    }

    public async Task<CreateTicketResponse> HandleAsync(CreateTicketCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.ClientName))
            throw new ArgumentException("ClientName is required.", nameof(command.ClientName));

        if (string.IsNullOrWhiteSpace(command.ProblemDescription))
            throw new ArgumentException("ProblemDescription is required.", nameof(command.ProblemDescription));

        if (!Enum.TryParse<Priority>(command.Priority, true, out var priority))
            throw new ArgumentException("Priority is invalid.", nameof(command.Priority));

        var ticket = new Ticket(
            command.ClientName,
            command.ProblemDescription,
            priority);      

        await _ticketRepository.AddAsync(ticket, cancellationToken);

        return new CreateTicketResponse(
            ticket.Id,
            ticket.ClientName,
            ticket.Status.ToString(),
            ticket.CreatedAt);
    }
}
