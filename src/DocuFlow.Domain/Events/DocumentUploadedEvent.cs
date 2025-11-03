using DocuFlow.Domain.Common;

namespace DocuFlow.Domain.Events;

public class DocumentUploadedEvent : DomainEvent
{
    public Guid DocumentId { get; }
    public Guid TenantId { get; }

    public DocumentUploadedEvent(Guid documentId, Guid tenantId)
    {
        DocumentId = documentId;
        TenantId = tenantId;
    }
}