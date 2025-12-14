using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using DocuFlow.Domain.Entities;
using DocuFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DocuFlow.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Documents
            .Include(d => d.ExtractionJobs)
                .ThenInclude(j => j.ExtractedFields)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<PaginatedList<Document>> GetByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Documents
            .Where(d => d.TenantId == tenantId)
            .OrderByDescending(d => d.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PaginatedList<Document>(items, total, page, pageSize);
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Documents.AnyAsync(d => d.Id == id, cancellationToken);

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.FindAsync(new object[] { id }, cancellationToken);
        if (document is not null)
        {
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateStatusAsync(Guid id, DocumentStatus status, CancellationToken cancellationToken = default)
    {
        await _context.Documents
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, status), cancellationToken);

        if (status == DocumentStatus.Completed)
        {
            await _context.Documents
                .Where(d => d.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.ProcessedAt, DateTime.UtcNow), cancellationToken);
        }
    }

    public async Task<DocumentStatsDto> GetStatsByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var docs = await _context.Documents
            .Where(d => d.TenantId == tenantId)
            .GroupBy(_ => 1)
            .Select(g => new DocumentStatsDto(
                g.Count(),
                g.Count(d => d.Status == DocumentStatus.Completed),
                g.Count(d => d.Status == DocumentStatus.Failed),
                g.Count(d => d.Status == DocumentStatus.Processing)
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return docs ?? new DocumentStatsDto(0, 0, 0, 0);
    }
}