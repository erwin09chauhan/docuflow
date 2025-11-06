using DocuFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocuFlow.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly Guid _tenantId;

    public AppDbContext(DbContextOptions<AppDbContext> options, Guid tenantId) : base(options)
    {
        _tenantId = tenantId;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ExtractionJob> ExtractionJobs => Set<ExtractionJob>();
    public DbSet<ExtractedField> ExtractedFields => Set<ExtractedField>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global query filter — tenants never see each other's data
        modelBuilder.Entity<Document>().HasQueryFilter(d => d.TenantId == _tenantId);
        modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == _tenantId);
        modelBuilder.Entity<ExtractionJob>().HasQueryFilter(e => e.TenantId == _tenantId);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(a => a.TenantId == _tenantId);

        base.OnModelCreating(modelBuilder);
    }
}