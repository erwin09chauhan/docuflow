using DocuFlow.Domain.Common;
using DocuFlow.Domain.Enums;

namespace DocuFlow.Domain.Entities;

public class ExtractionJob : BaseEntity
{
    public Guid DocumentId { get; private set; }
    public Guid TenantId { get; private set; }
    public ExtractionSchema Schema { get; private set; }
    public DocumentStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<ExtractedField> ExtractedFields { get; private set; } = new();

    private ExtractionJob() { }

    public static ExtractionJob Create(Guid documentId, Guid tenantId, ExtractionSchema schema)
    {
        return new ExtractionJob
        {
            DocumentId = documentId,
            TenantId = tenantId,
            Schema = schema,
            Status = DocumentStatus.Queued,
            AttemptCount = 0
        };
    }

    public void MarkProcessing()
    {
        Status = DocumentStatus.Processing;
        AttemptCount++;
    }

    public void MarkCompleted()
    {
        Status = DocumentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = DocumentStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }
}