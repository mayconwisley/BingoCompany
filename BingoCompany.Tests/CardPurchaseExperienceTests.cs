using BingoCompany.Domain.Entities;

namespace BingoCompany.Tests;

public sealed class CardPurchaseExperienceTests
{
	[Fact]
	public void OpenCardPurchase_configures_individual_limit_stock_alert_and_scheduled_closing()
	{
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa da empresa");
		var closesAt = DateTimeOffset.UtcNow.AddHours(2);

		bingoEvent.OpenCardPurchase(50, 5, closesAt, 8);

		Assert.True(bingoEvent.IsCardPurchaseAvailableAt(DateTimeOffset.UtcNow));
		Assert.Equal(5, bingoEvent.CardPurchasePerParticipantLimit);
		Assert.Equal(closesAt, bingoEvent.CardPurchaseClosesAt);
		Assert.Equal(8, bingoEvent.CardPurchaseLowStockThreshold);
		Assert.False(bingoEvent.IsCardPurchaseAvailableAt(closesAt));
	}

	[Fact]
	public void Invitation_can_only_be_redeemed_once()
	{
		var invitation = new CardPurchaseInvitation(Guid.CreateVersion7(), 1);

		invitation.Redeem(Guid.CreateVersion7());

		Assert.False(invitation.IsAvailableAt(DateTimeOffset.UtcNow));
		Assert.Throws<InvalidOperationException>(() => invitation.Redeem(Guid.CreateVersion7()));
	}
}
