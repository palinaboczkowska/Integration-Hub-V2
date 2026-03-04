using System.Net.Http.Json;
using IntegrationHub.Api.Integrations.Asana;
using IntegrationHub.Api.Integrations.Freshdesk;
using IntegrationHub.Api.Models;

namespace IntegrationHub.Api.Services;

public class TicketSyncService(
    FreshdeskClient freshdeskClient,
    AsanaClient asanaClient,
    ILogger<TicketSyncService> logger)
    : ITicketSyncService
{
    private readonly FreshdeskClient _freshdeskClient = freshdeskClient;
    private readonly AsanaClient _asanaClient = asanaClient;
    private readonly ILogger<TicketSyncService> _logger = logger;

    // Simple in-memory mapping store for now. Replace with persistent storage as needed.
    private readonly Dictionary<long, TicketSyncMapping> _byFreshdeskId = new();
    private readonly Dictionary<string, TicketSyncMapping> _byAsanaTaskId = new();

    public TicketSyncMapping? GetByFreshdeskId(long ticketId) =>
        _byFreshdeskId.TryGetValue(ticketId, out var mapping) ? mapping : null;

    public TicketSyncMapping? GetByAsanaTaskId(string taskId) =>
        _byAsanaTaskId.TryGetValue(taskId, out var mapping) ? mapping : null;

    public async Task<string> SyncFromFreshdeskTicketAsync(FreshdeskTicket ticket, CancellationToken cancellationToken = default)
    {
        var asanaPayload = new
        {
            data = new
            {
                name = ticket.Subject,
                notes = ticket.Description,
                workspace = "1213509069446553",
                projects = new[] { "1213508755299739" }
                // Optionally map more fields (assignee, due date, etc.)
            }
        };

        var response = await _asanaClient.CreateTask(asanaPayload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Failed to create Asana task for Freshdesk ticket {TicketId}. StatusCode: {StatusCode}, Body: {Body}",
                ticket.Id, (int)response.StatusCode, errorBody);

            throw new InvalidOperationException(
                $"Asana task creation failed with status {(int)response.StatusCode}: {errorBody}");
        }

        var created = await response.Content.ReadFromJsonAsync<AsanaTaskCreatedResponse>(cancellationToken: cancellationToken);
        if (created?.Data == null || string.IsNullOrWhiteSpace(created.Data.Gid))
        {
            _logger.LogWarning("Asana task creation succeeded but response body was unexpected for Freshdesk ticket {TicketId}", ticket.Id);
            throw new InvalidOperationException("Unable to read Asana task id from response.");
        }

        var mapping = new TicketSyncMapping
        {
            FreshdeskTicketId = ticket.Id,
            AsanaTaskId = created.Data.Gid,
            SyncedAtUtc = DateTime.UtcNow
        };

        _byFreshdeskId[ticket.Id] = mapping;
        _byAsanaTaskId[created.Data.Gid] = mapping;

        _logger.LogInformation("Synced Freshdesk ticket {TicketId} to Asana task {TaskId}", ticket.Id, created.Data.Gid);

        return created.Data.Gid;
    }

    public async Task<string> SyncFromFreshdeskTicketIdAsync(long ticketId, CancellationToken cancellationToken = default)
    {
        var existing = GetByFreshdeskId(ticketId);
        if (existing != null)
        {
            _logger.LogInformation("Mapping already exists for Freshdesk ticket {TicketId} -> Asana task {TaskId}", ticketId, existing.AsanaTaskId);
            return existing.AsanaTaskId;
        }

        var response = await _freshdeskClient.GetTicketById(ticketId, cancellationToken);
        response.EnsureSuccessStatusCode();

        var fdTicket = await response.Content.ReadFromJsonAsync<FreshdeskTicket>(cancellationToken: cancellationToken);
        if (fdTicket == null)
        {
            throw new InvalidOperationException($"Unable to deserialize Freshdesk ticket {ticketId}");
        }

        return await SyncFromFreshdeskTicketAsync(fdTicket, cancellationToken);
    }

    // Asana minimal response model for created task
    private sealed class AsanaTaskCreatedResponse
    {
        public AsanaTaskData? Data { get; set; }
    }

    private sealed class AsanaTaskData
    {
        public string Gid { get; set; } = string.Empty;
    }
}

