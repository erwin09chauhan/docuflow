using System.Text;
using System.Text.Json;
using DocuFlow.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace DocuFlow.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(HttpClient httpClient, ILogger<WebhookService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendAsync(string webhookUrl, object payload, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var attempt = 0;
        const int maxAttempts = 2;

        while (attempt < maxAttempts)
        {
            try
            {
                attempt++;
                var response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Webhook sent successfully to {WebhookUrl}", webhookUrl);
                    return;
                }

                _logger.LogWarning("Webhook attempt {Attempt} failed with status {Status}",
                    attempt, response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Webhook attempt {Attempt} threw an exception", attempt);
            }

            if (attempt < maxAttempts)
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }

        _logger.LogError("Webhook failed after {MaxAttempts} attempts to {WebhookUrl}",
            maxAttempts, webhookUrl);
    }
}