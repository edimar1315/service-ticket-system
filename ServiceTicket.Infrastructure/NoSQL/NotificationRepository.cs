// NoSQL/NotificationRepository.cs
using MongoDB.Driver;
using ServiceTicket.Core.Domain.Entities;
using ServiceTicket.Core.Domain.Interfaces;

namespace ServiceTicket.Infrastructure.NoSQL;

public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<TicketClosedNotification> _collection;

    public NotificationRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<TicketClosedNotification>("notifications");
    }

    public async Task AddAsync(TicketClosedNotification notification)
        => await _collection.InsertOneAsync(notification);

    public async Task<IEnumerable<TicketClosedNotification>> GetByTicketIdAsync(Guid ticketId)
    {
        var filter = Builders<TicketClosedNotification>
            .Filter.Eq(n => n.TicketId, ticketId);

        return await _collection.Find(filter).ToListAsync();
    }
}