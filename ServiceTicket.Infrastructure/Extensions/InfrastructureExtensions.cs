// Extensions/InfrastructureExtensions.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceTicket.Core.Domain.Interfaces;
using ServiceTicket.Infrastructure.Data;
using ServiceTicket.Infrastructure.Messaging;
using ServiceTicket.Infrastructure.NoSQL;
using ServiceTicket.Infrastructure.Repositories;

namespace ServiceTicket.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

        // MongoDB
        services.AddSingleton<MongoDbContext>();

        // RabbitMQ
        services.AddSingleton<RabbitMQConnection>();

        // Repositories
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IServiceOrderPublisher, ServiceOrderPublisher>();

        return services;
    }
}