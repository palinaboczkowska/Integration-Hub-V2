namespace IntegrationHub.Api.Models;

public class TicketSyncMapping
{
    public long FreshdeskTicketId { get; set; }
    public string AsanaTaskId { get; set; } = string.Empty;
    public DateTime SyncedAtUtc { get; set; } = DateTime.UtcNow;
}

