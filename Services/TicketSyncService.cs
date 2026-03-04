using System.Net.Http.Json;
using IntegrationHub.Api.Integrations.Asana;
using IntegrationHub.Api.Integrations.Freshdesk;
using IntegrationHub.Api.Models;
using IntegrationHub.Api.Repositories;

namespace IntegrationHub.Api.Services;

public class TicketSyncService(
    FreshdeskClient freshdeskClient,
    AsanaClient asanaClient,
    IMappingRepository mappingRepository,
    ILogger<TicketSyncService> logger)
    : ITicketSyncService
{
    private readonly FreshdeskClient _freshdeskClient = freshdeskClient;
    private readonly AsanaClient _asanaClient = asanaClient;
    private readonly IMappingRepository _mappingRepository = mappingRepository;
    private readonly ILogger<TicketSyncService> _logger = logger;

    public TicketSyncMapping? GetByFreshdeskId(long ticketId) =>
        _mappingRepository.GetByFreshdeskId(ticketId);

    public TicketSyncMapping? GetByAsanaTaskId(string taskId) =>
        _mappingRepository.GetByAsanaTaskId(taskId);

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

        await _mappingRepository.SaveAsync(mapping, cancellationToken);

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

