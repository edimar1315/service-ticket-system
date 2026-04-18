// UseCases/GetTickets/GetTicketsHandler.cs
using ServiceTicket.Core.Application.DTOs;
using ServiceTicket.Core.Domain.Enums;
using ServiceTicket.Core.Domain.Interfaces;

namespace ServiceTicket.Core.Application.UseCases.GetTickets;

public class GetTicketsHandler
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketsHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<IEnumerable<TicketDto>> HandleAsync(GetTicketsQuery query)
    {
        TicketStatus? status = null;
        if (!string.IsNullOrWhiteSpace(query.Status) && 
            Enum.TryParse<TicketStatus>(query.Status, true, out var parsedStatus))
        {
            status = parsedStatus;
        }

        Priority? priority = null;
        if (!string.IsNullOrWhiteSpace(query.Priority) && 
            Enum.TryParse<Priority>(query.Priority, true, out var parsedPriority))
        {
            priority = parsedPriority;
        }

        var tickets = await _ticketRepository.GetAllAsync(
            status,
            priority,
            query.ClientName);

        return tickets.Select(t => new TicketDto(
            t.Id,
            t.ClientName,
            t.ProblemDescription,
            t.Priority.ToString(),
            t.Status.ToString(),
            t.CreatedAt,
            t.UpdatedAt));
    }
}