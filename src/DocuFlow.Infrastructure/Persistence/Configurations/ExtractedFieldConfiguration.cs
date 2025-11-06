using DocuFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocuFlow.Infrastructure.Persistence.Configurations;

public class ExtractedFieldConfiguration : IEntityTypeConfiguration<ExtractedField>
{
    public void Configure(EntityTypeBuilder<ExtractedField> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.ExtractionJobId).IsRequired();
        builder.Property(f => f.FieldName).IsRequired().HasMaxLength(200);
        builder.Property(f => f.FieldValue).IsRequired().HasMaxLength(2000);
        builder.Property(f => f.ConfidenceScore).IsRequired();
        builder.Property(f => f.CreatedAt).IsRequired();
    }
}