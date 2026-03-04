using IntegrationHub.Api.Integrations.Freshdesk;
using IntegrationHub.Api.Integrations.Asana;
using IntegrationHub.Api.Models;
using IntegrationHub.Api.Services;
using IntegrationHub.Api.Data;
using IntegrationHub.Api.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Logging configuration leveraging environment-based settings
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configuration: strongly-typed options bound from environment-specific appsettings
builder.Services.Configure<IntegrationSettings>(
    builder.Configuration.GetSection("IntegrationSettings"));

// Add services to the container.
builder.Services.AddControllers();

// EF Core DbContext with SQLite
builder.Services.AddDbContext<IntegrationHubDbContext>(options =>
    options.UseSqlite("Data Source=integrationhub.db"));

// Dependency injection for core integration services
builder.Services.AddScoped<IIntegrationService, IntegrationService>();
builder.Services.AddScoped<IMappingRepository, EfMappingRepository>();
builder.Services.AddScoped<ITicketSyncService, TicketSyncService>();

// Typed HTTP client for Freshdesk integration
builder.Services.AddHttpClient<FreshdeskClient>();
// Typed HTTP client for Asana integration
builder.Services.AddHttpClient<AsanaClient>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Optionally expose Swagger in non-development with safeguards if desired
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
