using IntegrationHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntegrationHub.Api.Data;

public class IntegrationHubDbContext(DbContextOptions<IntegrationHubDbContext> options) : DbContext(options)
{
    public DbSet<TicketSyncMappingEntity> TicketSyncMappings => Set<TicketSyncMappingEntity>();
}

