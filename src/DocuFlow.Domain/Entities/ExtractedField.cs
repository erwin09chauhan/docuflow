using DocuFlow.Domain.Common;

namespace DocuFlow.Domain.Entities;

public class ExtractedField : BaseEntity
{
    public Guid ExtractionJobId { get; private set; }
    public string FieldName { get; private set; } = string.Empty;
    public string FieldValue { get; private set; } = string.Empty;
    public decimal ConfidenceScore { get; private set; }

    private ExtractedField() { }

    public static ExtractedField Create(Guid extractionJobId, string fieldName,
        string fieldValue, decimal confidenceScore)
    {
        if (confidenceScore < 0 || confidenceScore > 1)
            throw new ArgumentOutOfRangeException(nameof(confidenceScore),
                "Confidence score must be between 0 and 1");

        return new ExtractedField
        {
            ExtractionJobId = extractionJobId,
            FieldName = fieldName.Trim(),
            FieldValue = fieldValue.Trim(),
            ConfidenceScore = confidenceScore
        };
    }
}