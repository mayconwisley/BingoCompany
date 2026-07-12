using BingoCompany.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BingoCompany.Infrastructure.Persistence.Configurations;

public sealed class RoundEligibleCardConfiguration : IEntityTypeConfiguration<RoundEligibleCard>
{
    public void Configure(EntityTypeBuilder<RoundEligibleCard> builder)
    {
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RoundId, item.CardId }).IsUnique();
    }
}
