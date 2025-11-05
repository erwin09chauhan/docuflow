using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using MediatR;

namespace DocuFlow.Application.Queries.Documents;

public record GetDocumentByIdQuery(
    Guid DocumentId,
    Guid TenantId
) : IRequest<Result<DocumentDto>>;