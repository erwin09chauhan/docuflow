using DocuFlow.Domain.Enums;

namespace DocuFlow.Application.DTOs;

public record DocumentDto(
    Guid Id,
    string FileName,
    DocumentStatus Status,
    ExtractionSchema Schema,
    long SizeBytes,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);