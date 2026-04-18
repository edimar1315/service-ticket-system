// Domain/Interfaces/INotificationRepository.cs
namespace ServiceTicket.Core.Domain.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(TicketClosedNotification notification);
    Task<IEnumerable<TicketClosedNotification>> GetByTicketIdAsync(Guid ticketId);
}