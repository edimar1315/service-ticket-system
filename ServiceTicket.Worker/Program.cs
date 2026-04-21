using ServiceTicket.Core.Domain.Interfaces;
using ServiceTicket.Infrastructure.Messaging;
using ServiceTicket.Infrastructure.NoSQL;
using ServiceTicket.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<RabbitMQConnection>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
