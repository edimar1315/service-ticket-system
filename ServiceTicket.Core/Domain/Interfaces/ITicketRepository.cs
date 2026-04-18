// Domain/Interfaces/ITicketRepository.cs
namespace ServiceTicket.Core.Domain.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id);
    Task<IEnumerable<Ticket>> GetAllAsync(
        TicketStatus? status = null,
        Priority? priority = null,
        string? clientName = null);
    Task AddAsync(Ticket ticket);
    Task UpdateAsync(Ticket ticket);
}