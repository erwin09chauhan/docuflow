using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using DocuFlow.Domain.Enums;
using MediatR;

namespace DocuFlow.Application.Commands.Documents;

public record UploadDocumentCommand(
    Guid TenantId,
    Guid UploadedBy,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream FileStream,
    ExtractionSchema Schema
) : IRequest<Result<DocumentDto>>;