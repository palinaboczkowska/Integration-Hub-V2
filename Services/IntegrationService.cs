using IntegrationHub.Api.Models;
using Microsoft.Extensions.Options;

namespace IntegrationHub.Api.Services;

public class IntegrationService(IOptions<IntegrationSettings> options) : IIntegrationService
{
    private readonly IntegrationSettings _settings = options.Value;

    public IEnumerable<string> GetAvailableIntegrations()
    {
        // Seed with common integration targets; extend as needed.
        return new[]
        {
            "Asana",
            "Freshdesk",
            "Database"
        };
    }

    public IntegrationSettings GetSettings() => _settings;
}

