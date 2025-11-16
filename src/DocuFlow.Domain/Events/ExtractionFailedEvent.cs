using DocuFlow.Domain.Common;
using MediatR;

namespace DocuFlow.Domain.Events;

public class ExtractionFailedEvent : DomainEvent, INotification
{
    public Guid DocumentId { get; }
    public Guid TenantId { get; }
    public string ErrorMessage { get; }
    public int AttemptNumber { get; }

    public ExtractionFailedEvent(Guid documentId, Guid tenantId,
        string errorMessage, int attemptNumber)
    {
        DocumentId = documentId;
        TenantId = tenantId;
        ErrorMessage = errorMessage;
        AttemptNumber = attemptNumber;
    }
}