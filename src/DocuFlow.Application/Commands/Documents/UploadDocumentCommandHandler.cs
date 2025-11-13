using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using DocuFlow.Domain.Enums;
using MediatR;

namespace DocuFlow.Application.Commands.Documents;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Result<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IBackgroundJobService _backgroundJobService;

    public UploadDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        IBackgroundJobService backgroundJobService)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _backgroundJobService = backgroundJobService;
    }

    public async Task<Result<DocumentDto>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var uploadResult = await _fileStorageService.UploadAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            cancellationToken);

        var document = DocuFlow.Domain.Entities.Document.Create(
            request.TenantId,
            request.UploadedBy,
            request.FileName,
            uploadResult.FilePath,
            uploadResult.SizeBytes,
            request.ContentType,
            request.Schema);

        await _documentRepository.AddAsync(document, cancellationToken);

        // Queue background job
        _backgroundJobService.Enqueue(document.Id, request.TenantId);

        var dto = new DocumentDto(
            document.Id,
            document.FileName,
            document.Status,
            document.Schema,
            document.FileSizeBytes,
            document.CreatedAt,
            document.ProcessedAt);

        return Result<DocumentDto>.Success(dto);
    }
}