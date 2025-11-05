using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using MediatR;

namespace DocuFlow.Application.Queries.Documents;

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, Result<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentByIdQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Result<DocumentDto>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document is null || document.TenantId != request.TenantId)
            return Result<DocumentDto>.Failure("Document not found.");

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