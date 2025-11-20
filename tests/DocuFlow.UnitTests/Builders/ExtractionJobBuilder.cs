using DocuFlow.Domain.Entities;
using DocuFlow.Domain.Enums;

namespace DocuFlow.UnitTests.Builders;

public class ExtractionJobBuilder
{
    private Guid _documentId = Guid.NewGuid();
    private Guid _tenantId = Guid.NewGuid();
    private ExtractionSchema _schema = ExtractionSchema.Invoice;

    public ExtractionJobBuilder WithDocumentId(Guid documentId) { _documentId = documentId; return this; }
    public ExtractionJobBuilder WithTenantId(Guid tenantId) { _tenantId = tenantId; return this; }
    public ExtractionJobBuilder WithSchema(ExtractionSchema schema) { _schema = schema; return this; }

    public ExtractionJob Build() => ExtractionJob.Create(_documentId, _tenantId, _schema);
}