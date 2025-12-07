using DocuFlow.Domain.Entities;
using DocuFlow.Domain.Enums;

namespace DocuFlow.UnitTests.Builders;

public class DocumentBuilder
{
    private Guid _tenantId = Guid.NewGuid();
    private Guid _uploadedById = Guid.NewGuid();
    private string _fileName = "invoice.pdf";
    private string _filePath = "uploads/invoice.pdf";
    private long _fileSizeBytes = 1024;
    private string _mimeType = "application/pdf";
    private ExtractionSchema _schema = ExtractionSchema.Invoice;
    private DocumentStatus _status = DocumentStatus.Uploaded;

    public DocumentBuilder WithTenantId(Guid tenantId) { _tenantId = tenantId; return this; }
    public DocumentBuilder WithFileName(string fileName) { _fileName = fileName; return this; }
    public DocumentBuilder WithFilePath(string filePath) { _filePath = filePath; return this; }
    public DocumentBuilder WithSchema(ExtractionSchema schema) { _schema = schema; return this; }
    public DocumentBuilder WithStatus(DocumentStatus status) { _status = status; return this; }
    public DocumentBuilder WithMimeType(string mimeType) { _mimeType = mimeType; return this; }
    public Document Build()
    {
        var document = Document.Create(_tenantId, _uploadedById, _fileName, _filePath, _fileSizeBytes, _mimeType, _schema);

        if (_status == DocumentStatus.Queued)
            document.UpdateStatus(DocumentStatus.Queued);

        return document;
    }
}