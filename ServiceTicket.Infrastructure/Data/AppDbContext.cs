// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using ServiceTicket.Core.Domain.Entities;

namespace ServiceTicket.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<Ticket> Tickets => Set<Ticket>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}