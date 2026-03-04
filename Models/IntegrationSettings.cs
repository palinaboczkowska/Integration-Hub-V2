namespace IntegrationHub.Api.Models;

public class IntegrationSettings
{
    public string Name { get; set; } = "Integration Hub";
    public int DefaultTimeoutSeconds { get; set; } = 30;
    public string Environment { get; set; } = "Development";
}

