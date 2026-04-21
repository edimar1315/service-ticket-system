// UseCases/GetTickets/GetTicketsHandler.cs
using ServiceTicket.Core.Application.DTOs;
using ServiceTicket.Core.Domain.Enums;
using ServiceTicket.Core.Interfaces.Repositories;

namespace ServiceTicket.Core.Application.UseCases.GetTickets;

public class GetTicketsHandler
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketsHandler(ITicketRepository ticketRepository)
    {
        ArgumentNullException.ThrowIfNull(ticketRepository);
        _ticketRepository = ticketRepository;
    }

    public async Task<IEnumerable<TicketDto>> HandleAsync(GetTicketsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var status = ParseEnum<TicketStatus>(query.Status);
        var priority = ParseEnum<Priority>(query.Priority);

        var tickets = await _ticketRepository.GetAllAsync(
            status,
            priority,
            query.ClientName,
            pageNumber: 1,
            pageSize: 100,
            cancellationToken);

        return tickets.Select(t => new TicketDto(
            t.Id,
            t.ClientName,
            t.ProblemDescription,
            t.Priority.ToString(),
            t.Status.ToString(),
            t.CreatedAt,
            t.UpdatedAt));
    }

    private static TEnum? ParseEnum<TEnum>(string? value) where TEnum : struct, Enum
    {
        return !string.IsNullOrWhiteSpace(value) && Enum.TryParse<TEnum>(value, true, out var result)
            ? result
            : null;
    }
}