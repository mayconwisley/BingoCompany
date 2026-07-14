using System.Text.Json;
using BingoCompany.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BingoCompany.Infrastructure.Persistence.Configurations;

public sealed class BingoRoundConfiguration : IEntityTypeConfiguration<BingoRound>
{
    public void Configure(EntityTypeBuilder<BingoRound> builder)
    {
        builder.HasKey(item => item.Id);
        var drawSequence = builder.Property(item => item.DrawSequence).HasConversion(
            sequence => JsonSerializer.Serialize(sequence, JsonSerializerOptions.Default),
            json => json == null ? null : JsonSerializer.Deserialize<int[]>(json, JsonSerializerOptions.Default));
        drawSequence.Metadata.SetValueComparer(new ValueComparer<int[]?>(
            (left, right) => left == right || (left != null && right != null && left.SequenceEqual(right)),
            sequence => sequence == null ? 0 : sequence.Aggregate(0, HashCode.Combine),
            sequence => sequence == null ? null : sequence.ToArray()));
        builder.HasMany(item => item.Stages).WithOne().HasForeignKey(item => item.RoundId);
        builder.HasMany(item => item.DrawnNumbers).WithOne().HasForeignKey(item => item.RoundId);
        builder.HasMany(item => item.EligibleCards).WithOne().HasForeignKey(item => item.RoundId);
    }
}
