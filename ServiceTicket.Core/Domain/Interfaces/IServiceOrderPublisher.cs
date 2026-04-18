// Domain/Interfaces/IServiceOrderPublisher.cs
namespace ServiceTicket.Core.Domain.Interfaces;

public interface IServiceOrderPublisher
{
    Task PublishAsync(Guid ticketId);
}