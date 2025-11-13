using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Domain.Entities;
using DocuFlow.Domain.Enums;
using DocuFlow.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocuFlow.Infrastructure.Services;

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IExtractionJobRepository _extractionJobRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAiExtractionService _aiExtractionService;
    private readonly IEmailService _emailService;
    private readonly IPublisher _publisher;
    private readonly ILogger<DocumentProcessingService> _logger;

    public DocumentProcessingService(
        IDocumentRepository documentRepository,
        IExtractionJobRepository extractionJobRepository,
        IFileStorageService fileStorageService,
        IAiExtractionService aiExtractionService,
        IEmailService emailService,
        IPublisher publisher,
        ILogger<DocumentProcessingService> logger)
    {
        _documentRepository = documentRepository;
        _extractionJobRepository = extractionJobRepository;
        _fileStorageService = fileStorageService;
        _aiExtractionService = aiExtractionService;
        _emailService = emailService;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task ProcessAsync(Guid documentId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting processing for document {DocumentId}", documentId);

        // Step 1 — Fetch document
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
        {
            _logger.LogError("Document {DocumentId} not found", documentId);
            return;
        }

        // Step 2 — Update status to Processing
        document.UpdateStatus(DocumentStatus.Processing);
        await _documentRepository.UpdateAsync(document, cancellationToken);

        ExtractionJob? job = null;

        try
        {
            // Step 3 — Download file from blob storage
            var fileStream = await _fileStorageService.DownloadAsync(document.FilePath, cancellationToken);
            using var reader = new StreamReader(fileStream);
            var fileContent = await reader.ReadToEndAsync(cancellationToken);

            // Step 4 — Call AI extraction
            var request = new ExtractionRequest(fileContent, document.Schema, document.TenantInstructions);
            var extractionResult = await _aiExtractionService.ExtractAsync(request, cancellationToken);

            if (!extractionResult.Success)
                throw new Exception(extractionResult.ErrorMessage);

            // Step 5 — Fetch extraction job and mark completed
            job = await _extractionJobRepository.GetLatestByDocumentIdAsync(documentId, cancellationToken);
            if (job is not null)
            {
                job.MarkCompleted();
                await _extractionJobRepository.UpdateAsync(job, cancellationToken);
            }

            // Step 6 — Update document status to Completed
            document.UpdateStatus(DocumentStatus.Completed);
            await _documentRepository.UpdateAsync(document, cancellationToken);

            // Step 7 — Dispatch domain event
            await _publisher.Publish(
                new ExtractionCompletedEvent(documentId, tenantId, extractionResult.Fields.Count),
                cancellationToken);

            _logger.LogInformation("Document {DocumentId} processed successfully", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Processing failed for document {DocumentId}", documentId);

            document.UpdateStatus(DocumentStatus.Failed);
            await _documentRepository.UpdateAsync(document, cancellationToken);

            await _publisher.Publish(
                new ExtractionFailedEvent(documentId, tenantId, ex.Message, job?.AttemptCount ?? 1),
                cancellationToken);

            throw; // rethrow so Hangfire knows to retry
        }
    }
}