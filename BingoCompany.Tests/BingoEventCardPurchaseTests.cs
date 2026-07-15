using BingoCompany.Domain.Entities;

namespace BingoCompany.Tests;

public sealed class BingoEventCardPurchaseTests
{
	[Fact]
	public void UpdateCardPurchaseLimit_does_not_allow_a_limit_below_cards_already_sold()
	{
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		bingoEvent.OpenCardPurchase(10);

		var exception = Assert.Throws<InvalidOperationException>(() => bingoEvent.UpdateCardPurchaseLimit(4, 5));

		Assert.Equal("A quantidade não pode ser menor que as cartelas já vendidas.", exception.Message);
	}

	[Fact]
	public void CancelCardPurchase_keeps_the_reason_and_prevents_reopening_sales()
	{
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		bingoEvent.OpenCardPurchase(10);

		bingoEvent.CancelCardPurchase("Evento adiado");

		Assert.False(bingoEvent.IsCardPurchaseOpen);
		Assert.Equal("Evento adiado", bingoEvent.CardPurchaseCancellationReason);
		Assert.Throws<InvalidOperationException>(() => bingoEvent.OpenCardPurchase(10));
	}

	[Fact]
	public void Opening_public_registration_prevents_opening_card_purchase()
	{
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		bingoEvent.OpenRegistration(3);

		Assert.Equal(3, bingoEvent.CardsPerParticipant);
		Assert.Throws<InvalidOperationException>(() => bingoEvent.OpenCardPurchase(10));
	}

	[Fact]
	public void Opening_card_purchase_prevents_public_registration()
	{
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		bingoEvent.OpenCardPurchase(10);

		Assert.Throws<InvalidOperationException>(() => bingoEvent.OpenRegistration(3));
	}
}
