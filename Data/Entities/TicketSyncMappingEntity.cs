namespace IntegrationHub.Api.Data.Entities;

public class TicketSyncMappingEntity
{
    public int Id { get; set; }

    public long FreshdeskTicketId { get; set; }

    public string AsanaTaskId { get; set; } = string.Empty;

    public DateTime SyncedAtUtc { get; set; }
}

