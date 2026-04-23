using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceTicket.API.Models;
using ServiceTicket.Core.Application.DTOs;
using ServiceTicket.Core.Domain.Entities;
using ServiceTicket.Core.Domain.Enums;
using ServiceTicket.Core.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ServiceTicket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IMapper _mapper;
    private readonly ILogger<TicketsController> _logger;
    private readonly UserManager<User> _userManager;

    public TicketsController(
        ITicketService ticketService,
        IMapper mapper,
        ILogger<TicketsController> logger,
        UserManager<User> userManager)
    {
        _ticketService = ticketService;
        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
    }

    private Guid CurrentUserId
    {
        get
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(userIdClaim, out var userId)
                ? userId
                : throw new InvalidOperationException("UserId claim não encontrado.");
        }
    }

    /// <summary>
    /// Verifica se o usuário autenticado tem a role Support.
    /// Consulta o banco como fonte autoritativa para contornar tokens
    /// emitidos antes da atribuição correta da role.
    /// </summary>
    private async Task<bool> IsSupportAsync()
    {
        // Verificação rápida pelo token primeiro
        if (User.IsInRole(UserRole.Support) ||
            User.HasClaim(c =>
                (c.Type == "role" || c.Type == ClaimTypes.Role) &&
                c.Value == UserRole.Support))
        {
            return true;
        }

        // Fallback: consulta direta ao banco
        var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
        if (user == null) return false;
        return await _userManager.IsInRoleAsync(user, UserRole.Support);
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
                CurrentUserId,
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

            // Customer só pode ver seus próprios tickets
            if (!await IsSupportAsync() && ticket.CreatedByUserId != CurrentUserId)
                return Forbid();

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

            // Customer vê apenas seus tickets; Support vê todos
            Guid? createdByUserId = await IsSupportAsync() ? null : CurrentUserId;

            var (tickets, totalCount) = await _ticketService.GetTicketsAsync(
                status,
                priority,
                filter.ClientName,
                filter.PageNumber,
                filter.PageSize,
                createdByUserId,
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
    /// Atribui um ticket ao analista de suporte autenticado.
    /// </summary>
    [HttpPatch("{id:guid}/assign")]
    [Authorize]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTicket(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await IsSupportAsync())
                return Forbid();

            _logger.LogInformation("Analista {UserId} assumindo ticket {TicketId}", CurrentUserId, id);

            var ticket = await _ticketService.AssignTicketAsync(id, CurrentUserId, cancellationToken);

            var response = _mapper.Map<TicketDto>(ticket);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operação inválida ao atribuir ticket {TicketId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atribuir ticket {TicketId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao atribuir ticket. Tente novamente mais tarde." });
        }
    }

    /// <summary>
    /// Atualiza o status de um ticket. Apenas Support.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTicketStatus(
        Guid id,
        [FromBody] UpdateTicketStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await IsSupportAsync())
                return Forbid();

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

    /// <summary>
    /// Obtém métricas agregadas dos tickets. Apenas Support.
    /// </summary>
    [HttpGet("metrics")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        try
        {
            if (!await IsSupportAsync())
                return Forbid();

            _logger.LogDebug("Obtendo métricas de tickets");

            var (tickets, _) = await _ticketService.GetTicketsAsync(
                null, null, null, 1, int.MaxValue, null, cancellationToken);

            var metrics = new
            {
                total = tickets.Count(),
                open = tickets.Count(t => t.Status == TicketStatus.Open),
                inProgress = tickets.Count(t => t.Status == TicketStatus.InProgress),
                finished = tickets.Count(t => t.Status == TicketStatus.Finished),
                cancelled = tickets.Count(t => t.Status == TicketStatus.Cancelled)
            };

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter métricas");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao obter métricas. Tente novamente mais tarde." });
        }
    }

    /// <summary>
    /// Obtém tickets agrupados por analista de suporte. Apenas Support.
    /// </summary>
    [HttpGet("analysts")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTicketsByAnalysts(CancellationToken cancellationToken)
    {
        try
        {
            if (!await IsSupportAsync())
                return Forbid();

            _logger.LogDebug("Obtendo tickets agrupados por analista");

            var ticketsByAnalyst = await _ticketService.GetTicketsByAnalystsAsync(cancellationToken);

            return Ok(ticketsByAnalyst);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter tickets por analista");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao obter tickets por analista. Tente novamente mais tarde." });
        }
    }
}

