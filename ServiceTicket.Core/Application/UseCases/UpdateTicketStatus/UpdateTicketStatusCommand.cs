// UseCases/UpdateTicketStatus/UpdateTicketStatusCommand.cs
using ServiceTicket.Core.Domain.Enums;

namespace ServiceTicket.Core.Application.UseCases.UpdateTicketStatus;

public record UpdateTicketStatusCommand(
    Guid TicketId,
    TicketStatus NewStatus
);