using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using MediatR;

namespace DocuFlow.Application.Queries.Documents;

public record GetDocumentsQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<DocumentDto>>>;