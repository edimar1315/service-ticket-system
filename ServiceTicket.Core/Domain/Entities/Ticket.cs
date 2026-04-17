using System;

namespace ServiceTicket.Core.Domain.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ProblemDescription { get; set; } = string.Empty;
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public string? AssignedToUserId { get; set; }
        
        
        public void UpdateStatus(TicketStatus newStatus)
        {
            if (newStatus != Status)
            {
                Status = newStatus;
                UpdatedAt = DateTime.UtcNow;

                if (newStatus == TicketStatus.Closed)
                {
                    ClosedAt = DateTime.UtcNow;
                }
            }
        }
        
        public void UpdatePriority(TicketPriority newPriority)
        {
            Priority = newPriority;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public enum TicketPriority
        {
            Low = 1,
            Medium = 2,
            High = 3,
            Critical = 4
        }

        public enum TicketStatus 
        {
            Open = 1,
            InProgress = 2,
            Resolved = 3,
            Closed = 4
        }

    }
}