using BingoCompany.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BingoCompany.Infrastructure.Persistence.Configurations;

public sealed class CardPurchaseWaitlistEntryConfiguration : IEntityTypeConfiguration<CardPurchaseWaitlistEntry>
{
	public void Configure(EntityTypeBuilder<CardPurchaseWaitlistEntry> builder)
	{
		builder.HasKey(item => item.Id);
		builder.HasIndex(item => new { item.EventId, item.ParticipantAccountId }).IsUnique();
		builder.HasIndex(item => new { item.EventId, item.CreatedAt });
		builder.HasOne<BingoEvent>().WithMany().HasForeignKey(item => item.EventId);
		builder.HasOne<ParticipantAccount>().WithMany().HasForeignKey(item => item.ParticipantAccountId);
	}
}
