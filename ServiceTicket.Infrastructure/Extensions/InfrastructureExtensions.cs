// Extensions/InfrastructureExtensions.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceTicket.Core.Interfaces.Repositories;
using ServiceTicket.Core.Interfaces.Messaging;
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
        services.AddScoped<IMessagePublisher, RabbitMQPublisher>();

        // Repositories
        services.AddScoped<ITicketRepository, TicketRepository>();

        return services;
    }
}