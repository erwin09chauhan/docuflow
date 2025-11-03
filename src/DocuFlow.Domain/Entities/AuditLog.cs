using DocuFlow.Domain.Common;

namespace DocuFlow.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(Guid tenantId, Guid? userId, string action,
        string entityType, Guid entityId, string? metadataJson = null)
    {
        return new AuditLog
        {
            TenantId = tenantId,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            MetadataJson = metadataJson,
            CreatedAt = DateTime.UtcNow
        };
    }
}