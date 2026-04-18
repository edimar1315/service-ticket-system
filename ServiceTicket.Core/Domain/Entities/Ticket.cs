// Domain/Entities/Ticket.cs
namespace ServiceTicket.Core.Domain.Entities;

public class Ticket
{
    public Guid Id { get; private set; }
    public string ClientName { get; private set; }
    public string ProblemDescription { get; private set; }
    public Priority Priority { get; private set; }
    public TicketStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Ticket() { } // EF Core

    public Ticket(string clientName, string problemDescription, Priority priority)
    {
        if (string.IsNullOrWhiteSpace(clientName))
            throw new ArgumentException("Cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(problemDescription))
            throw new ArgumentException("Descrição do problema é obrigatória.");

        Id = Guid.NewGuid();
        ClientName = clientName;
        ProblemDescription = problemDescription;
        Priority = priority;
        Status = TicketStatus.Open;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(TicketStatus newStatus)
    {
        if (Status == TicketStatus.Finished)
            throw new InvalidOperationException("Chamado finalizado não pode ser alterado.");

        if (Status == TicketStatus.Cancelled)
            throw new InvalidOperationException("Chamado cancelado não pode ser alterado.");

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }
}