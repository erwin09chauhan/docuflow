using DocuFlow.Domain.Common;

namespace DocuFlow.Domain.Entities;

public class ExtractionJob : BaseEntity
{
    public Guid DocumentId { get; private set; }
    public int AttemptNumber { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool IsSuccessful { get; private set; }

    private ExtractionJob() { }

    public static ExtractionJob Create(Guid documentId, int attemptNumber)
    {
        return new ExtractionJob
        {
            DocumentId = documentId,
            AttemptNumber = attemptNumber,
            StartedAt = DateTime.UtcNow,
            IsSuccessful = false
        };
    }

    public void MarkSuccessful()
    {
        IsSuccessful = true;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string errorMessage)
    {
        IsSuccessful = false;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }
}