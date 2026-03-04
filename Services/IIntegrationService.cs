using IntegrationHub.Api.Models;

namespace IntegrationHub.Api.Services;

public interface IIntegrationService
{
    IEnumerable<string> GetAvailableIntegrations();
    IntegrationSettings GetSettings();
}

