namespace DocuFlow.Application.DTOs;

public record ExtractedFieldDto(
    Guid Id,
    string FieldName,
    string FieldValue,
    decimal ConfidenceScore
);