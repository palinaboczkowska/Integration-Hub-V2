using IntegrationHub.Api.Models;

namespace IntegrationHub.Api.Services;

public interface ITicketSyncService
{
    Task<string> SyncFromFreshdeskTicketAsync(FreshdeskTicket ticket, CancellationToken cancellationToken = default);
    Task<string> SyncFromFreshdeskTicketIdAsync(long ticketId, CancellationToken cancellationToken = default);

    TicketSyncMapping? GetByFreshdeskId(long ticketId);
    TicketSyncMapping? GetByAsanaTaskId(string taskId);
}

