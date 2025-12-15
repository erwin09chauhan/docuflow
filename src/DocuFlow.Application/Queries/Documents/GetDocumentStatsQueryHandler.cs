using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using DocuFlow.Domain.Enums;
using MediatR;

namespace DocuFlow.Application.Queries.Documents;

public class GetDocumentStatsQueryHandler : IRequestHandler<GetDocumentStatsQuery, Result<DocumentStatsDto>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentStatsQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Result<DocumentStatsDto>> Handle(GetDocumentStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _documentRepository.GetStatsByTenantAsync(request.TenantId, cancellationToken);
        return Result<DocumentStatsDto>.Success(stats);
    }
}