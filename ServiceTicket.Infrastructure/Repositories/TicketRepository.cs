// Repositories/TicketRepository.cs
using Microsoft.EntityFrameworkCore;
using ServiceTicket.Core.Domain.Entities;
using ServiceTicket.Core.Domain.Enums;
using ServiceTicket.Core.Domain.Interfaces;
using ServiceTicket.Infrastructure.Data;

namespace ServiceTicket.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
        => await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Ticket>> GetAllAsync(
        TicketStatus? status = null,
        Priority? priority = null,
        string? clientName = null)
    {
        var query = _context.Tickets.AsNoTracking();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (!string.IsNullOrWhiteSpace(clientName))
            query = query.Where(t => t.ClientName
                .ToLower()
                .Contains(clientName.ToLower()));

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }
}