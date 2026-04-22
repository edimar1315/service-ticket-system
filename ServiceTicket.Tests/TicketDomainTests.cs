using Microsoft.Extensions.Logging.Abstractions;
using ServiceTicket.API.Services;
using ServiceTicket.Core.Domain.Entities;
using ServiceTicket.Core.Domain.Enums;
using ServiceTicket.Core.Events;
using ServiceTicket.Core.Interfaces.Messaging;
using ServiceTicket.Core.Interfaces.Repositories;

namespace ServiceTicket.Tests;

public class TicketDomainTests
{
    [Fact]
    public void Constructor_ShouldCreateTicketWithOpenStatus()
    {
        var userId = Guid.NewGuid();
        var ticket = new Ticket("Cliente A", "Sem conexão", Priority.High, userId);

        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Equal("Cliente A", ticket.ClientName);
        Assert.Equal(userId, ticket.CreatedByUserId);
    }

    [Fact]
    public void UpdateStatus_WhenTicketIsFinished_ShouldThrow()
    {
        var ticket = new Ticket("Cliente A", "Sem conexão", Priority.High, Guid.NewGuid());
        ticket.UpdateStatus(TicketStatus.Finished);

        Assert.Throws<InvalidOperationException>(() => ticket.UpdateStatus(TicketStatus.InProgress));
    }
}

public class TicketApplicationServiceTests
{
    [Fact]
    public async Task UpdateTicketStatusAsync_WhenTicketNotFound_ShouldThrow()
    {
        var service = CreateService(new InMemoryTicketRepository(), new FakeMessagePublisher());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateTicketStatusAsync(Guid.NewGuid(), TicketStatus.InProgress));
    }

    [Fact]
    public async Task UpdateTicketStatusAsync_WhenFinished_ShouldPublishEvent()
    {
        var repository = new InMemoryTicketRepository();
        var publisher = new FakeMessagePublisher();
        var service = CreateService(repository, publisher);

        var ticket = new Ticket("Cliente B", "Impressora falhando", Priority.Medium, Guid.NewGuid());
        await repository.AddAsync(ticket);

        var updated = await service.UpdateTicketStatusAsync(ticket.Id, TicketStatus.Finished);

        Assert.Equal(TicketStatus.Finished, updated.Status);
        Assert.Single(publisher.PublishedMessages);
        Assert.Equal("service-ticket.ticket-finalized", publisher.PublishedMessages[0].RoutingKey);
        Assert.IsType<TicketFinalizedEvent>(publisher.PublishedMessages[0].Message);
    }

    private static TicketApplicationService CreateService(ITicketRepository repository, IMessagePublisher publisher)
        => new(repository, publisher, NullLogger<TicketApplicationService>.Instance);

    private sealed class InMemoryTicketRepository : ITicketRepository
    {
        private readonly Dictionary<Guid, Ticket> _tickets = new();

        public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_tickets.GetValueOrDefault(id));

        public Task<IEnumerable<Ticket>> GetAllAsync(TicketStatus? status = null, Priority? priority = null, string? clientName = null, int pageNumber = 1, int pageSize = 10, Guid? createdByUserId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(_tickets.Values.AsEnumerable());

        public Task<int> GetTotalCountAsync(TicketStatus? status = null, Priority? priority = null, string? clientName = null, Guid? createdByUserId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(_tickets.Count);

        public Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
        {
            _tickets[ticket.Id] = ticket;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
        {
            _tickets[ticket.Id] = ticket;
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_tickets.ContainsKey(id));
    }

    private sealed class FakeMessagePublisher : IMessagePublisher
    {
        public List<(object Message, string RoutingKey)> PublishedMessages { get; } = [];

        public Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default) where T : class
        {
            PublishedMessages.Add((message, routingKey));
            return Task.CompletedTask;
        }
    }
}