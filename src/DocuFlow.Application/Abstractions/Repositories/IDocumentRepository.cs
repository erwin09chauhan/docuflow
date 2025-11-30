using DocuFlow.Application.Common;
using DocuFlow.Domain.Enums;

namespace DocuFlow.Application.Abstractions.Repositories;

public interface IDocumentRepository
{
    Task<DocuFlow.Domain.Entities.Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedList<DocuFlow.Domain.Entities.Document>> GetByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(DocuFlow.Domain.Entities.Document document, CancellationToken cancellationToken = default);
    Task UpdateAsync(DocuFlow.Domain.Entities.Document document, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid id, DocumentStatus status, CancellationToken cancellationToken = default);
}