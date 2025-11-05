using DocuFlow.Domain.Enums;

namespace DocuFlow.Application.Abstractions.Services;

public record ExtractionRequest(
    string DocumentContent,
    ExtractionSchema Schema,
    string? TenantInstructions = null
);

public record ExtractedFieldResult(
    string FieldName,
    string FieldValue,
    double ConfidenceScore
);

public record ExtractionServiceResult(
    bool Success,
    List<ExtractedFieldResult> Fields,
    string? ErrorMessage = null
);

public interface IAiExtractionService
{
    Task<ExtractionServiceResult> ExtractAsync(
        ExtractionRequest request,
        CancellationToken cancellationToken = default);
}