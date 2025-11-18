using DocuFlow.Domain.Common;
using MediatR;

namespace DocuFlow.Domain.Events;

public class ExtractionCompletedEvent : DomainEvent, INotification
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