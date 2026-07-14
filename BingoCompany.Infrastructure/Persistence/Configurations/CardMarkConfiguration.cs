using BingoCompany.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BingoCompany.Infrastructure.Persistence.Configurations;

public sealed class CardMarkConfiguration : IEntityTypeConfiguration<CardMark>
{
    public void Configure(EntityTypeBuilder<CardMark> builder)
    {
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RoundId, item.CardId, item.Number }).IsUnique();
    }
}
