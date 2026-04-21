using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceTicket.API.Models;
using ServiceTicket.Core.Application.DTOs;
using ServiceTicket.Core.Domain.Enums;
using ServiceTicket.Core.Interfaces.Services;

namespace ServiceTicket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // TODO: Descomentar quando configurar autenticação JWT
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IMapper _mapper;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(
        ITicketService ticketService,
        IMapper mapper,
        ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo ticket
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTicket(
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Requisição de criação de ticket recebida para cliente: {ClientName}", request.ClientName);

            var priority = (Priority)request.Priority;
            var ticket = await _ticketService.CreateTicketAsync(
                request.ClientName,
                request.ProblemDescription,
                priority,
                cancellationToken);

            var response = _mapper.Map<TicketDto>(ticket);

            return CreatedAtAction(
                nameof(GetTicketById),
                new { id = ticket.Id },
                response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar ticket");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar ticket");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao criar ticket. Tente novamente mais tarde." });
        }
    }

    /// <summary>
    /// Obtém um ticket específico por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTicketById(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Buscando ticket com ID: {TicketId}", id);

            var ticket = await _ticketService.GetTicketByIdAsync(id, cancellationToken);

            if (ticket == null)
            {
                _logger.LogWarning("Ticket {TicketId} não encontrado", id);
                return NotFound(new { message = $"Ticket {id} não encontrado." });
            }

            var response = _mapper.Map<TicketDto>(ticket);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar ticket {TicketId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao buscar ticket. Tente novamente mais tarde." });
        }
    }

    /// <summary>
    /// Lista tickets com filtros opcionais
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTickets(
        [FromQuery] TicketFilterRequest filter,
        CancellationToken cancellationToken)
    {
        try
        {
            if (filter.PageNumber < 1 || filter.PageSize < 1 || filter.PageSize > 100)
            {
                return BadRequest(new { message = "Parâmetros de paginação inválidos." });
            }

            _logger.LogDebug("Listando tickets - Página: {PageNumber}, Tamanho: {PageSize}", filter.PageNumber, filter.PageSize);

            TicketStatus? status = filter.Status.HasValue ? (TicketStatus)filter.Status.Value : null;
            Priority? priority = filter.Priority.HasValue ? (Priority)filter.Priority.Value : null;

            var (tickets, totalCount) = await _ticketService.GetTicketsAsync(
                status,
                priority,
                filter.ClientName,
                filter.PageNumber,
                filter.PageSize,
                cancellationToken);

            var ticketDtos = _mapper.Map<IEnumerable<TicketDto>>(tickets);

            var result = new
            {
                data = ticketDtos,
                totalCount,
                pageNumber = filter.PageNumber,
                pageSize = filter.PageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar tickets");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao listar tickets. Tente novamente mais tarde." });
        }
    }

    /// <summary>
    /// Atualiza o status de um ticket
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateTicketStatus(
        Guid id,
        [FromBody] UpdateTicketStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Requisição de atualização de status do ticket {TicketId} para {NewStatus}", id, request.Status);

            var newStatus = (TicketStatus)request.Status;
            var ticket = await _ticketService.UpdateTicketStatusAsync(id, newStatus, cancellationToken);

            var response = _mapper.Map<TicketDto>(ticket);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operação inválida ao atualizar status do ticket {TicketId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status do ticket {TicketId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao atualizar status. Tente novamente mais tarde." });
        }
    }
}
