using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Application.Common;
using DocuFlow.Domain.Entities;
using DocuFlow.Domain.Enums;
using MediatR;

namespace DocuFlow.Application.Commands.Extraction;

public class ProcessExtractionCommandHandler : IRequestHandler<ProcessExtractionCommand, Result>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IExtractionJobRepository _extractionJobRepository;
    private readonly IAiExtractionService _aiExtractionService;
    private readonly IFileStorageService _fileStorageService;

    public ProcessExtractionCommandHandler(
        IDocumentRepository documentRepository,
        IExtractionJobRepository extractionJobRepository,
        IAiExtractionService aiExtractionService,
        IFileStorageService fileStorageService)
    {
        _documentRepository = documentRepository;
        _extractionJobRepository = extractionJobRepository;
        _aiExtractionService = aiExtractionService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> Handle(ProcessExtractionCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return Result.Failure("Document not found.");

        var existingJobs = await _extractionJobRepository.GetByDocumentIdAsync(document.Id, cancellationToken);
        var attemptNumber = existingJobs.Count + 1;

        var job = ExtractionJob.Create(document.Id, attemptNumber);
        await _extractionJobRepository.AddAsync(job, cancellationToken);

        document.UpdateStatus(DocumentStatus.Processing);
        await _documentRepository.UpdateAsync(document, cancellationToken);

        var fileStream = await _fileStorageService.DownloadAsync(document.FilePath, cancellationToken);
        using var reader = new StreamReader(fileStream);
        var content = await reader.ReadToEndAsync(cancellationToken);

        var extractionRequest = new ExtractionRequest(content, document.Schema, document.TenantInstructions);
        var extractionResult = await _aiExtractionService.ExtractAsync(extractionRequest, cancellationToken);

        if (!extractionResult.Success)
        {
            document.UpdateStatus(DocumentStatus.Failed);
            job.MarkFailed(extractionResult.ErrorMessage!);

            await _documentRepository.UpdateAsync(document, cancellationToken);
            await _extractionJobRepository.UpdateAsync(job, cancellationToken);

            return Result.Failure(extractionResult.ErrorMessage!);
        }

        document.UpdateStatus(DocumentStatus.Completed);
        job.MarkSuccessful();

        await _documentRepository.UpdateAsync(document, cancellationToken);
        await _extractionJobRepository.UpdateAsync(job, cancellationToken);

        return Result.Success();
    }
}