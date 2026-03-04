using IntegrationHub.Api.Data;
using IntegrationHub.Api.Data.Entities;
using IntegrationHub.Api.Models;

namespace IntegrationHub.Api.Repositories;

public class EfMappingRepository(IntegrationHubDbContext dbContext) : IMappingRepository
{
    private readonly IntegrationHubDbContext _dbContext = dbContext;

    public TicketSyncMapping? GetByFreshdeskId(long ticketId)
    {
        var entity = _dbContext.TicketSyncMappings
            .FirstOrDefault(m => m.FreshdeskTicketId == ticketId);

        return entity == null ? null : MapToModel(entity);
    }

    public TicketSyncMapping? GetByAsanaTaskId(string taskId)
    {
        var entity = _dbContext.TicketSyncMappings
            .FirstOrDefault(m => m.AsanaTaskId == taskId);

        return entity == null ? null : MapToModel(entity);
    }

    public async Task SaveAsync(TicketSyncMapping mapping, CancellationToken cancellationToken = default)
    {
        var entity = _dbContext.TicketSyncMappings
            .FirstOrDefault(m => m.FreshdeskTicketId == mapping.FreshdeskTicketId);

        if (entity == null)
        {
            entity = new TicketSyncMappingEntity
            {
                FreshdeskTicketId = mapping.FreshdeskTicketId,
                AsanaTaskId = mapping.AsanaTaskId,
                SyncedAtUtc = mapping.SyncedAtUtc
            };

            _dbContext.TicketSyncMappings.Add(entity);
        }
        else
        {
            entity.AsanaTaskId = mapping.AsanaTaskId;
            entity.SyncedAtUtc = mapping.SyncedAtUtc;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static TicketSyncMapping MapToModel(TicketSyncMappingEntity entity) =>
        new()
        {
            FreshdeskTicketId = entity.FreshdeskTicketId,
            AsanaTaskId = entity.AsanaTaskId,
            SyncedAtUtc = entity.SyncedAtUtc
        };
}

