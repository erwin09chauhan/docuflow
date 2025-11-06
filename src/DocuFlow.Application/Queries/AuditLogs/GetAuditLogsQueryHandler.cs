using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using MediatR;

namespace DocuFlow.Application.Queries.AuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<PaginatedList<AuditLogDto>>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<Result<PaginatedList<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _auditLogRepository.GetByTenantAsync(
            request.TenantId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = logs.Items.Select(l => new AuditLogDto(
            l.Id,
            l.UserId,
            l.Action,
            l.EntityType,
            l.EntityId,
            l.Details,
            l.CreatedAt)).ToList();

        var paginated = new PaginatedList<AuditLogDto>(dtos, logs.TotalCount, logs.Page, logs.PageSize);

        return Result<PaginatedList<AuditLogDto>>.Success(paginated);
    }
}