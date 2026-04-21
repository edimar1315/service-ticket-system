using ServiceTicket.API.Services;
using ServiceTicket.Core.Interfaces.Services;
using ServiceTicket.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure (DbContext, Repositories, RabbitMQ, MongoDB)
builder.Services.AddInfrastructure(builder.Configuration);

// Application Services
builder.Services.AddScoped<ITicketService, TicketApplicationService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Service Ticket API", Version = "v1" });
});

// Authentication & Authorization (opcional por enquanto)
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options => { ... });
// builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// app.UseAuthentication(); // Descomente quando configurar JWT
// app.UseAuthorization();  // Descomente quando configurar JWT

app.MapControllers();

app.Run();
