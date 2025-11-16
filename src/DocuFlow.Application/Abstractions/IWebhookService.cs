namespace DocuFlow.Application.Abstractions.Services;

public interface IWebhookService
{
    Task SendAsync(string webhookUrl, object payload, CancellationToken cancellationToken = default);
}