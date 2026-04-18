// Domain/Interfaces/INotificationRepository.cs
using ServiceTicket.Core.Domain.Entities;

namespace ServiceTicket.Core.Domain.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(TicketClosedNotification notification);
    Task<IEnumerable<TicketClosedNotification>> GetByTicketIdAsync(Guid ticketId);
}
