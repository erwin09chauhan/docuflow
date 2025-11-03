using DocuFlow.Domain.Common;

namespace DocuFlow.Domain.Events;

public class ExtractionCompletedEvent : DomainEvent
{
    public Guid DocumentId { get; }
    public Guid TenantId { get; }
    public int ExtractedFieldCount { get; }

    public ExtractionCompletedEvent(Guid documentId, Guid tenantId, int extractedFieldCount)
    {
        DocumentId = documentId;
        TenantId = tenantId;
        ExtractedFieldCount = extractedFieldCount;
    }
}