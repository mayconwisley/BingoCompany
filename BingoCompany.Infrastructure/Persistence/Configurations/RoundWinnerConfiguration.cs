using BingoCompany.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BingoCompany.Infrastructure.Persistence.Configurations;

public sealed class RoundWinnerConfiguration : IEntityTypeConfiguration<RoundWinner>
{
	public void Configure(EntityTypeBuilder<RoundWinner> builder)
	{
		builder.HasKey(item => item.Id);
		builder.HasIndex(item => new { item.StageId, item.CardId }).IsUnique();
		builder.Property(item => item.RevealedAt).IsConcurrencyToken();
		builder.Property(item => item.TieBreakerNumber).IsConcurrencyToken();
	}
}
