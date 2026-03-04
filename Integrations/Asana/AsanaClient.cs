using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationHub.Api.Integrations.Asana;

public class AsanaClient
{
    private readonly HttpClient _httpClient;

    public AsanaClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        var baseUrl = configuration["Asana:BaseUrl"] ?? "https://app.asana.com/api/1.0";

        var token = configuration["Asana:Token"] ?? "YOUR_ASANA_TOKEN";

        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // Важно: пути без начального "/" чтобы корректно дополнять BaseAddress "/api/1.0"
    public Task<HttpResponseMessage> CreateTask(object taskPayload, CancellationToken cancellationToken = default) =>
        _httpClient.PostAsJsonAsync("tasks", taskPayload, cancellationToken);

    public Task<HttpResponseMessage> UpdateTask(string taskId, object taskPayload, CancellationToken cancellationToken = default) =>
        _httpClient.PutAsJsonAsync($"tasks/{taskId}", taskPayload, cancellationToken);

    public Task<HttpResponseMessage> AddComment(string taskId, object commentPayload, CancellationToken cancellationToken = default) =>
        _httpClient.PostAsJsonAsync($"tasks/{taskId}/stories", commentPayload, cancellationToken);
}

