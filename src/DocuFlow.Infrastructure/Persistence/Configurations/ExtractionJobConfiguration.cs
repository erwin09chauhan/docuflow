using DocuFlow.Domain.Entities;
using DocuFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocuFlow.Infrastructure.Persistence.Configurations;

public class ExtractionJobConfiguration : IEntityTypeConfiguration<ExtractionJob>
{
    public void Configure(EntityTypeBuilder<ExtractionJob> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.DocumentId).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Schema).IsRequired().HasConversion<string>();
        builder.Property(e => e.Status).IsRequired().HasConversion<string>();
        builder.Property(e => e.AttemptCount).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasMany(e => e.ExtractedFields)
            .WithOne()
            .HasForeignKey(f => f.ExtractionJobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}