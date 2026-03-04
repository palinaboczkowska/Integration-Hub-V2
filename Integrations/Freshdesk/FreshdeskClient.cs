using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationHub.Api.Integrations.Freshdesk;

public class FreshdeskClient
{
    private readonly HttpClient _httpClient;

    public FreshdeskClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        // Configuration placeholders; set real values in appsettings or environment variables.
        var baseUrl = configuration["Freshdesk:BaseUrl"] ?? "https://your-domain.freshdesk.com";
        var apiKey = configuration["Freshdesk:ApiKey"] ?? "YOUR_FRESHDESK_API_KEY";

        _httpClient.BaseAddress = new Uri(baseUrl);

        // Freshdesk uses API key as basic auth username with 'X' or blank password.
        var byteArray = System.Text.Encoding.ASCII.GetBytes($"{apiKey}:X");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<HttpResponseMessage> GetTicketById(long id, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync($"/api/v2/tickets/{id}", cancellationToken);
    }

    public async Task<HttpResponseMessage> ListTickets(CancellationToken cancellationToken = default)
    {
        // Adjust query parameters as needed (e.g. filter, pagination).
        return await _httpClient.GetAsync("/api/v2/tickets", cancellationToken);
    }

    public async Task<HttpResponseMessage> CreateTicket(object ticketPayload, CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAsJsonAsync("/api/v2/tickets", ticketPayload, cancellationToken);
    }
}

