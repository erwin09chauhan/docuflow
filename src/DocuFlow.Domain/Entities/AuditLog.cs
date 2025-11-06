using DocuFlow.Domain.Common;

namespace DocuFlow.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string? Details { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(Guid tenantId, Guid userId, string action,
        string entityType, Guid entityId, string? details = null)
    {
        return new AuditLog
        {
            TenantId = tenantId,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details
        };
    }
}