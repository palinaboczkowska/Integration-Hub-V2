using IntegrationHub.Api.Models;

namespace IntegrationHub.Api.Repositories;

public interface IMappingRepository
{
    TicketSyncMapping? GetByFreshdeskId(long ticketId);
    TicketSyncMapping? GetByAsanaTaskId(string taskId);
    Task SaveAsync(TicketSyncMapping mapping, CancellationToken cancellationToken = default);
}

