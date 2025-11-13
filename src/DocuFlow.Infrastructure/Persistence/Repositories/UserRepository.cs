using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocuFlow.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
      => await _context.DomainUsers.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default)
        => await _context.DomainUsers.FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId, cancellationToken);

    public async Task<List<User>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await _context.DomainUsers.Where(u => u.TenantId == tenantId).ToListAsync(cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.DomainUsers.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await _context.DomainUsers.AddAsync(user, cancellationToken);

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.DomainUsers.Update(user);
        return Task.CompletedTask;
    }
}