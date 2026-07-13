using BingoCompany.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BingoCompany.Infrastructure.Persistence.Configurations;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.EventId, item.OccurredAt });
        builder.Property(item => item.Action).HasMaxLength(120);
    }
}
