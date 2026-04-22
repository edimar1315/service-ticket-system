// Repositories/TicketRepository.cs
using Microsoft.EntityFrameworkCore;
using ServiceTicket.Core.Domain.Entities;
using ServiceTicket.Core.Domain.Enums;
using ServiceTicket.Core.Interfaces.Repositories;
using ServiceTicket.Infrastructure.Data;

namespace ServiceTicket.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IEnumerable<Ticket>> GetAllAsync(
        TicketStatus? status = null,
        Priority? priority = null,
        string? clientName = null,
        int pageNumber = 1,
        int pageSize = 10,
        Guid? createdByUserId = null,
        CancellationToken cancellationToken = default)
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

        if (createdByUserId.HasValue)
            query = query.Where(t => t.CreatedByUserId == createdByUserId.Value);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(
        TicketStatus? status = null,
        Priority? priority = null,
        string? clientName = null,
        Guid? createdByUserId = null,
        CancellationToken cancellationToken = default)
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

        if (createdByUserId.HasValue)
            query = query.Where(t => t.CreatedByUserId == createdByUserId.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        await _context.Tickets.AddAsync(ticket, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        // O ticket já está sendo rastreado pelo contexto (GetByIdAsync sem AsNoTracking)
        // SaveChanges detecta automaticamente as mudanças da entidade rastreada
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Tickets
            .AsNoTracking()
            .AnyAsync(t => t.Id == id, cancellationToken);
}