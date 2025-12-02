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

        // Step 1 — Transition to Queued
        await _documentRepository.UpdateStatusAsync(documentId, DocumentStatus.Queued, cancellationToken);

        // Step 2 — Transition to Processing
        await _documentRepository.UpdateStatusAsync(documentId, DocumentStatus.Processing, cancellationToken);

        // Step 3 — Fetch job
        var job = await _extractionJobRepository.GetLatestByDocumentIdAsync(documentId, cancellationToken);
        if (job is null)
        {
            _logger.LogError("No extraction job found for document {DocumentId}", documentId);
            await _documentRepository.UpdateStatusAsync(documentId, DocumentStatus.Failed, cancellationToken);
            return;
        }

        job.MarkProcessing();
        await _extractionJobRepository.UpdateAsync(job, cancellationToken);

        try
        {
            // Step 4 — Fetch document for file path and schema
            var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
            if (document is null)
            {
                _logger.LogError("Document {DocumentId} not found", documentId);
                return;
            }

            // Step 5 — Download file
            var fileStream = await _fileStorageService.DownloadAsync(document.FilePath, cancellationToken);
            using var reader = new StreamReader(fileStream);
            var fileContent = await reader.ReadToEndAsync(cancellationToken);

            // Step 6 — Call AI extraction
            var request = new ExtractionRequest(fileContent, document.Schema, document.TenantInstructions);
            var extractionResult = await _aiExtractionService.ExtractAsync(request, cancellationToken);

            if (!extractionResult.Success)
                throw new Exception(extractionResult.ErrorMessage);

            // Step 7 — Save extracted fields
            var extractedFields = extractionResult.Fields
                .Select(f => ExtractedField.Create(job.Id, f.FieldName, f.FieldValue, (decimal)f.ConfidenceScore))
                .ToList();
            await _extractionJobRepository.AddExtractedFieldsAsync(extractedFields, cancellationToken);

            // Step 8 — Mark job completed
            job.MarkCompleted();
            await _extractionJobRepository.UpdateAsync(job, cancellationToken);

            // Step 9 — Update document status
            await _documentRepository.UpdateStatusAsync(documentId, DocumentStatus.Completed, cancellationToken);

            // Step 10 — Dispatch domain event
            await _publisher.Publish(
                new ExtractionCompletedEvent(documentId, tenantId, extractedFields.Count),
                cancellationToken);

            _logger.LogInformation("Document {DocumentId} processed successfully", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Processing failed for document {DocumentId}", documentId);

            job.MarkFailed(ex.Message);
            await _extractionJobRepository.UpdateAsync(job, cancellationToken);

            await _documentRepository.UpdateStatusAsync(documentId, DocumentStatus.Failed, cancellationToken);

            await _publisher.Publish(
                new ExtractionFailedEvent(documentId, tenantId, ex.Message, job.AttemptCount),
                cancellationToken);

            throw;
        }
    }
}