using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using MediatR;

namespace DocuFlow.Application.Queries.Extraction;

public class GetExtractedFieldsQueryHandler : IRequestHandler<GetExtractedFieldsQuery, Result<List<ExtractedFieldDto>>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetExtractedFieldsQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Result<List<ExtractedFieldDto>>> Handle(GetExtractedFieldsQuery request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document is null || document.TenantId != request.TenantId)
            return Result<List<ExtractedFieldDto>>.Failure("Document not found.");

        var dtos = document.ExtractionJobs
            .SelectMany(j => j.ExtractedFields)
            .Select(f => new ExtractedFieldDto(
                f.Id,
                f.FieldName,
                f.FieldValue,
                f.ConfidenceScore))
            .ToList();

        return Result<List<ExtractedFieldDto>>.Success(dtos);
    }
}