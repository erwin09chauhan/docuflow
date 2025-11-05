using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using MediatR;

namespace DocuFlow.Application.Queries.Documents;

public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, Result<PaginatedList<DocumentDto>>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentsQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Result<PaginatedList<DocumentDto>>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetByTenantAsync(
            request.TenantId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = documents.Items.Select(d => new DocumentDto(
            d.Id,
            d.FileName,
            d.Status,
            d.Schema,
            d.FileSizeBytes,
            d.CreatedAt,
            d.ProcessedAt)).ToList();

        var paginated = new PaginatedList<DocumentDto>(dtos, documents.TotalCount, documents.Page, documents.PageSize);

        return Result<PaginatedList<DocumentDto>>.Success(paginated);
    }
}