using ServiceTicket.Core.Domain.Entities;
using ServiceTicket.Core.Domain.Enums;

namespace ServiceTicket.Core.Interfaces.Services;

public interface ITicketService
{
    Task<Ticket> CreateTicketAsync(string clientName, string problemDescription, Priority priority, CancellationToken cancellationToken = default);
    Task<Ticket?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Ticket> Tickets, int TotalCount)> GetTicketsAsync(
        TicketStatus? status = null,
        Priority? priority = null,
        string? clientName = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
    Task<Ticket> UpdateTicketStatusAsync(Guid id, TicketStatus newStatus, CancellationToken cancellationToken = default);
}
