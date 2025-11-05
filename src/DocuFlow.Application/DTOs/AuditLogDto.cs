namespace DocuFlow.Application.DTOs;

public record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string Action,
    string EntityType,
    Guid EntityId,
    string? Metadata,
    DateTime CreatedAt
);