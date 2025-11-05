using DocuFlow.Domain.Entities;

namespace DocuFlow.Application.Abstractions.Repositories;

public interface IExtractionJobRepository
{
    Task<ExtractionJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ExtractionJob>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task AddAsync(ExtractionJob job, CancellationToken cancellationToken = default);
    Task UpdateAsync(ExtractionJob job, CancellationToken cancellationToken = default);
}