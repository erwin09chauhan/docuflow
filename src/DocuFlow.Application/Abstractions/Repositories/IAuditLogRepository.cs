using DocuFlow.Domain.Entities;
using DocuFlow.Application.Common;

namespace DocuFlow.Application.Abstractions.Repositories;

public interface IAuditLogRepository
{
    Task<PaginatedList<AuditLog>> GetByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}