using BingoCompany.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BingoCompany.Infrastructure.Persistence.Configurations;

public sealed class CardPurchaseInvitationConfiguration : IEntityTypeConfiguration<CardPurchaseInvitation>
{
	public void Configure(EntityTypeBuilder<CardPurchaseInvitation> builder)
	{
		builder.HasKey(item => item.Id);
		builder.Property(item => item.Code).HasMaxLength(12);
		builder.HasIndex(item => item.Code).IsUnique();
		builder.HasIndex(item => new { item.EventId, item.RedeemedByParticipantAccountId });
		builder.HasOne<BingoEvent>().WithMany(item => item.CardPurchaseInvitations).HasForeignKey(item => item.EventId);
	}
}
