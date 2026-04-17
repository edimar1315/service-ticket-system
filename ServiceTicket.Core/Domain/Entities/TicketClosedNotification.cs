using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace ServiceTicket.Core.Domain.Entities
{
    public class TicketClosedNotification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string TicketId { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ProblemDescription { get; set; } = string.Empty;
        public DateTime ClosedAt { get; set; }
        public DateTime ProcessedAt { get; set; }
        public bool EmailSent { get; set; }
        public string NotificationMessage { get; set; } = string.Empty;

    }
}