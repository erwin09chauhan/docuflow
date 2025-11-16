using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocuFlow.Application.EventHandlers;

public class ExtractionCompletedEventHandler : INotificationHandler<ExtractionCompletedEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IEmailService _emailService;
    private readonly IWebhookService _webhookService;
    private readonly ILogger<ExtractionCompletedEventHandler> _logger;

    public ExtractionCompletedEventHandler(
        IUserRepository userRepository,
        IDocumentRepository documentRepository,
        ITenantRepository tenantRepository,
        IEmailService emailService,
        IWebhookService webhookService,
        ILogger<ExtractionCompletedEventHandler> logger)
    {
        _userRepository = userRepository;
        _documentRepository = documentRepository;
        _tenantRepository = tenantRepository;
        _emailService = emailService;
        _webhookService = webhookService;
        _logger = logger;
    }

    public async Task Handle(ExtractionCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ExtractionCompletedEvent for document {DocumentId}",
            notification.DocumentId);

        var document = await _documentRepository.GetByIdAsync(notification.DocumentId, cancellationToken);
        if (document is null) return;

        var user = await _userRepository.GetByIdAsync(document.UploadedById, cancellationToken);
        if (user is null) return;

        // Send email
        await _emailService.SendAsync(new EmailMessage(
            user.Email,
            "Document Processing Complete",
            $"Your document '{document.FileName}' has been processed successfully. " +
            $"{notification.ExtractedFieldCount} fields were extracted."),
            cancellationToken);

        // Fire webhook if configured
        var tenant = await _tenantRepository.GetByIdAsync(notification.TenantId, cancellationToken);
        if (tenant?.WebhookUrl is not null)
        {
            var payload = new
            {
                @event = "extraction.completed",
                documentId = notification.DocumentId,
                tenantId = notification.TenantId,
                extractedFieldCount = notification.ExtractedFieldCount,
                occurredAt = notification.OccurredAt
            };

            await _webhookService.SendAsync(tenant.WebhookUrl, payload, cancellationToken);
        }
    }
}