using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using MediatR;

namespace DocuFlow.Application.Queries.Extraction;

public record GetExtractedFieldsQuery(
    Guid DocumentId,
    Guid TenantId
) : IRequest<Result<List<ExtractedFieldDto>>>;