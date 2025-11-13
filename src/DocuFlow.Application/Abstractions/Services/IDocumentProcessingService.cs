namespace DocuFlow.Application.Abstractions.Services;

public interface IDocumentProcessingService
{
    Task ProcessAsync(Guid documentId, Guid tenantId, CancellationToken cancellationToken = default);
}