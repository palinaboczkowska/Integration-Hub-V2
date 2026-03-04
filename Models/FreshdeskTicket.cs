namespace IntegrationHub.Api.Models;

public class FreshdeskTicket
{
    public long Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

