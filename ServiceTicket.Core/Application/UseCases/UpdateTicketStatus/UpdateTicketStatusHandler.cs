// UseCases/UpdateTicketStatus/UpdateTicketStatusHandler.cs
using ServiceTicket.Core.Domain.Enums;
using ServiceTicket.Core.Domain.Interfaces;

namespace ServiceTicket.Core.Application.UseCases.UpdateTicketStatus;

public class UpdateTicketStatusHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IServiceOrderPublisher _publisher;

    public UpdateTicketStatusHandler(
        ITicketRepository ticketRepository,
        IServiceOrderPublisher publisher)
    {
        ArgumentNullException.ThrowIfNull(ticketRepository);
        ArgumentNullException.ThrowIfNull(publisher);
        _ticketRepository = ticketRepository;
        _publisher = publisher;
    }

    public async Task HandleAsync(UpdateTicketStatusCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken)
            ?? throw new KeyNotFoundException($"Chamado {command.TicketId} não encontrado.");

        ticket.UpdateStatus(command.NewStatus);
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);

        // Publica na fila quando finalizado — Worker irá consumir
        if (command.NewStatus == TicketStatus.Finished)
            await _publisher.PublishAsync(ticket.Id, cancellationToken);
    }
}