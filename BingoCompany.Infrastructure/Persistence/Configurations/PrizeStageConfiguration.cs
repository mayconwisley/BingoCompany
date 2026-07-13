using BingoCompany.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BingoCompany.Infrastructure.Persistence.Configurations;

public sealed class PrizeStageConfiguration : IEntityTypeConfiguration<PrizeStage>
{
    public void Configure(EntityTypeBuilder<PrizeStage> builder) => builder.Property(item => item.PrizeImageDataUrl).HasMaxLength(2_800_000);
}
