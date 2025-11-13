using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Common;
using DocuFlow.Domain.Entities;
using DocuFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DocuFlow.Infrastructure.Persistence.Repositories;

public class ExtractionJobRepository : IExtractionJobRepository
{
    private readonly AppDbContext _context;

    public ExtractionJobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ExtractionJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.ExtractionJobs
            .Include(e => e.ExtractedFields)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<List<ExtractionJob>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
        => await _context.ExtractionJobs
            .Include(e => e.ExtractedFields)
            .Where(e => e.DocumentId == documentId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<ExtractionJob?> GetLatestByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    => await _context.ExtractionJobs
        .Include(e => e.ExtractedFields)
        .Where(e => e.DocumentId == documentId)
        .OrderByDescending(e => e.CreatedAt)
        .FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<ExtractionJob>> GetPendingJobsAsync(CancellationToken cancellationToken = default)
        => await _context.ExtractionJobs
            .Where(e => e.Status == DocumentStatus.Queued)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ExtractionJob job, CancellationToken cancellationToken = default)
        => await _context.ExtractionJobs.AddAsync(job, cancellationToken);

    public Task UpdateAsync(ExtractionJob job, CancellationToken cancellationToken = default)
    {
        _context.ExtractionJobs.Update(job);
        return Task.CompletedTask;
    }
}