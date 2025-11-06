using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Common;
using DocuFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocuFlow.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AuditLog>> GetByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .Where(a => a.TenantId == tenantId)
            .OrderByDescending(a => a.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PaginatedList<AuditLog>(items, total, page, pageSize);
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        => await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
}