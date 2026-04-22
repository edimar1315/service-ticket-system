using Microsoft.AspNetCore.Identity;
using ServiceTicket.Core.Application.DTOs;
using ServiceTicket.Core.Domain.Entities;
using ServiceTicket.Core.Domain.Enums;
using ServiceTicket.Core.Events;
using ServiceTicket.Core.Interfaces.Messaging;
using ServiceTicket.Core.Interfaces.Repositories;
using ServiceTicket.Core.Interfaces.Services;

namespace ServiceTicket.API.Services;

public class TicketApplicationService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<TicketApplicationService> _logger;
    private readonly UserManager<User> _userManager;

    public TicketApplicationService(
        ITicketRepository ticketRepository,
        IMessagePublisher messagePublisher,
        ILogger<TicketApplicationService> logger,
        UserManager<User> userManager)
    {
        _ticketRepository = ticketRepository;
        _messagePublisher = messagePublisher;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<Ticket> CreateTicketAsync(
        string clientName,
        string problemDescription,
        Priority priority,
        Guid createdByUserId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Criando novo ticket para cliente: {ClientName}", clientName);

        var ticket = new Ticket(clientName, problemDescription, priority, createdByUserId);
        await _ticketRepository.AddAsync(ticket, cancellationToken);

        _logger.LogInformation("Ticket {TicketId} criado com sucesso", ticket.Id);

        return ticket;
    }

    public async Task<Ticket?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Buscando ticket com ID: {TicketId}", id);
        return await _ticketRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<(IEnumerable<Ticket> Tickets, int TotalCount)> GetTicketsAsync(
        TicketStatus? status = null,
        Priority? priority = null,
        string? clientName = null,
        int pageNumber = 1,
        int pageSize = 10,
        Guid? createdByUserId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Listando tickets com filtros - Status: {Status}, Prioridade: {Priority}, Cliente: {ClientName}",
            status, priority, clientName);

        var tickets = await _ticketRepository.GetAllAsync(status, priority, clientName, pageNumber, pageSize, createdByUserId, cancellationToken);
        var totalCount = await _ticketRepository.GetTotalCountAsync(status, priority, clientName, createdByUserId, cancellationToken);

        return (tickets, totalCount);
    }

    public async Task<Ticket> UpdateTicketStatusAsync(
        Guid id,
        TicketStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Atualizando status do ticket {TicketId} para {NewStatus}", id, newStatus);

        var ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} não encontrado", id);
            throw new InvalidOperationException($"Ticket {id} não encontrado.");
        }

        var previousStatus = ticket.Status;
        ticket.UpdateStatus(newStatus);
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);

        _logger.LogInformation("Status do ticket {TicketId} atualizado de {PreviousStatus} para {NewStatus}",
            id, previousStatus, newStatus);

        // Publicar evento se o ticket foi finalizado
        if (newStatus == TicketStatus.Finished)
        {
            await PublishTicketFinalizedEventAsync(ticket, cancellationToken);
        }

        return ticket;
    }

    public async Task<Ticket> AssignTicketAsync(
        Guid id,
        Guid supportUserId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Atribuindo ticket {TicketId} ao analista {UserId}", id, supportUserId);

        var ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} não encontrado para atribuição", id);
            throw new InvalidOperationException($"Ticket {id} não encontrado.");
        }

        ticket.AssignTo(supportUserId);
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);

        _logger.LogInformation("Ticket {TicketId} atribuído ao analista {UserId}", id, supportUserId);

        return ticket;
    }

    public async Task<IEnumerable<TicketsByAnalystDto>> GetTicketsByAnalystsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obtendo tickets agrupados por analista");

        var allTickets = await _ticketRepository.GetAllAsync(
            status: null,
            priority: null,
            clientName: null,
            pageNumber: 1,
            pageSize: int.MaxValue,
            createdByUserId: null,
            cancellationToken: cancellationToken);

        // Agrupar tickets por analista atribuído
        var ticketsByAnalyst = allTickets
            .Where(t => t.AssignedToUserId.HasValue)
            .GroupBy(t => t.AssignedToUserId!.Value)
            .ToList();

        var result = new List<TicketsByAnalystDto>();

        foreach (var group in ticketsByAnalyst)
        {
            var analystId = group.Key;
            var analystUser = await _userManager.FindByIdAsync(analystId.ToString());

            if (analystUser == null)
                continue;

            var tickets = group.ToList();
            var openCount = tickets.Count(t => t.Status == TicketStatus.Open);
            var inProgressCount = tickets.Count(t => t.Status == TicketStatus.InProgress);
            var finishedCount = tickets.Count(t => t.Status == TicketStatus.Finished);

            var ticketSummaries = tickets.Select(t => new TicketSummaryDto(
                Id: t.Id,
                ClientName: t.ClientName,
                Priority: t.Priority.ToString(),
                Status: t.Status.ToString(),
                CreatedAt: t.CreatedAt,
                UpdatedAt: t.UpdatedAt
            )).ToList();

            result.Add(new TicketsByAnalystDto(
                AnalystId: analystUser.Id,
                AnalystName: analystUser.FullName,
                AnalystEmail: analystUser.Email ?? "N/A",
                TotalTickets: tickets.Count,
                OpenCount: openCount,
                InProgressCount: inProgressCount,
                FinishedCount: finishedCount,
                Tickets: ticketSummaries
            ));
        }

        return result.OrderBy(a => a.AnalystName);
    }

    private async Task PublishTicketFinalizedEventAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        try
        {
            var ticketFinalizedEvent = new TicketFinalizedEvent
            {
                TicketId = ticket.Id,
                ClientName = ticket.ClientName,
                FinalizedAt = ticket.UpdatedAt ?? DateTime.UtcNow,
                Status = ticket.Status.ToString()
            };

            await _messagePublisher.PublishAsync(ticketFinalizedEvent, "service-ticket.ticket-finalized", cancellationToken);

            _logger.LogInformation("Evento de finalização publicado para o ticket {TicketId}", ticket.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento de finalização do ticket {TicketId}", ticket.Id);
            // Não propaga a exceção para não falhar a transação principal
        }
    }
}
