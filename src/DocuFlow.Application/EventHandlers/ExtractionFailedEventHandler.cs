using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocuFlow.Application.EventHandlers;

public class ExtractionFailedEventHandler : INotificationHandler<ExtractionFailedEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IEmailService _emailService;
    private readonly IWebhookService _webhookService;
    private readonly ILogger<ExtractionFailedEventHandler> _logger;

    public ExtractionFailedEventHandler(
        IUserRepository userRepository,
        IDocumentRepository documentRepository,
        ITenantRepository tenantRepository,
        IEmailService emailService,
        IWebhookService webhookService,
        ILogger<ExtractionFailedEventHandler> logger)
    {
        _userRepository = userRepository;
        _documentRepository = documentRepository;
        _tenantRepository = tenantRepository;
        _emailService = emailService;
        _webhookService = webhookService;
        _logger = logger;
    }

    public async Task Handle(ExtractionFailedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ExtractionFailedEvent for document {DocumentId}",
            notification.DocumentId);

        var document = await _documentRepository.GetByIdAsync(notification.DocumentId, cancellationToken);
        if (document is null) return;

        var user = await _userRepository.GetByIdAsync(document.UploadedById, cancellationToken);
        if (user is null) return;

        // Send email
        await _emailService.SendAsync(new EmailMessage(
            user.Email,
            "Document Processing Failed",
            $"Your document '{document.FileName}' failed to process on attempt {notification.AttemptNumber}. " +
            $"Error: {notification.ErrorMessage}"),
            cancellationToken);

        // Fire webhook if configured
        var tenant = await _tenantRepository.GetByIdAsync(notification.TenantId, cancellationToken);
        if (tenant?.WebhookUrl is not null)
        {
            var payload = new
            {
                @event = "extraction.failed",
                documentId = notification.DocumentId,
                tenantId = notification.TenantId,
                errorMessage = notification.ErrorMessage,
                attemptNumber = notification.AttemptNumber,
                occurredAt = notification.OccurredAt
            };

            await _webhookService.SendAsync(tenant.WebhookUrl, payload, cancellationToken);
        }
    }
}