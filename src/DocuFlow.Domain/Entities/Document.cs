using DocuFlow.Domain.Common;
using DocuFlow.Domain.Enums;
using DocuFlow.Domain.Events;

namespace DocuFlow.Domain.Entities;

public class Document : AuditableEntity
{
    public Guid TenantId { get; private set; }
    public Guid UploadedById { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string MimeType { get; private set; } = string.Empty;
    public DocumentStatus Status { get; private set; }
    public ExtractionSchema Schema { get; private set; }
    public string? TenantInstructions { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private readonly List<ExtractionJob> _extractionJobs = new();
    private readonly List<ExtractedField> _extractedFields = new();

    public IReadOnlyCollection<ExtractionJob> ExtractionJobs => _extractionJobs.AsReadOnly();
    public IReadOnlyCollection<ExtractedField> ExtractedFields => _extractedFields.AsReadOnly();

    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Document() { }

    public static Document Create(Guid tenantId, Guid uploadedById, string fileName,
        string filePath, long fileSizeBytes, string mimeType,
        ExtractionSchema schema, string? tenantInstructions = null)
    {
        var document = new Document
        {
            TenantId = tenantId,
            UploadedById = uploadedById,
            FileName = fileName,
            FilePath = filePath,
            FileSizeBytes = fileSizeBytes,
            MimeType = mimeType,
            Status = DocumentStatus.Uploaded,
            Schema = schema,
            TenantInstructions = tenantInstructions
        };

        document._domainEvents.Add(new DocumentUploadedEvent(document.Id, tenantId));

        return document;
    }

    public void UpdateStatus(DocumentStatus newStatus)
    {
        // Failed can be reached from any state (background job error handling)
        if (newStatus == DocumentStatus.Failed)
        {
            Status = DocumentStatus.Failed;
            return;
        }

        // Idempotency — already in target state, do nothing
        if (Status == newStatus)
            return;

        var validTransitions = new Dictionary<DocumentStatus, List<DocumentStatus>>
    {
        { DocumentStatus.Uploaded, new List<DocumentStatus> { DocumentStatus.Queued } },
        { DocumentStatus.Queued,   new List<DocumentStatus> { DocumentStatus.Processing } },
        { DocumentStatus.Processing, new List<DocumentStatus> { DocumentStatus.Completed } },
        { DocumentStatus.Failed,   new List<DocumentStatus> { DocumentStatus.Queued } }
    };

        if (!validTransitions.ContainsKey(Status) || !validTransitions[Status].Contains(newStatus))
            throw new InvalidOperationException($"Invalid status transition from {Status} to {newStatus}");

        Status = newStatus;

        if (newStatus == DocumentStatus.Completed)
            ProcessedAt = DateTime.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    public void UpdateSchema(ExtractionSchema schema) => Schema = schema;
    public void UpdateTenantInstructions(string instructions) => TenantInstructions = instructions;
}