using BingoCompany.Domain;
using BingoCompany.Domain.Entities;

namespace BingoCompany.Tests;

public sealed class BingoCardInvalidationTests
{
	[Fact]
	public void InvalidateUnused_cancels_a_digital_card_waiting_for_activation()
	{
		var card = new BingoCard(Guid.CreateVersion7(), Guid.CreateVersion7(), CardType.Digital, CreateCard(), activateImmediately: false);

		card.InvalidateUnused();

		Assert.Equal(CardStatus.Cancelled, card.Status);
	}

	[Fact]
	public void InvalidateForCancelledSale_cancels_an_active_card_and_keeps_the_reason()
	{
		var card = new BingoCard(Guid.CreateVersion7(), Guid.CreateVersion7(), CardType.Digital, CreateCard());

		card.InvalidateForCancelledSale("Evento adiado");

		Assert.Equal(CardStatus.Cancelled, card.Status);
		Assert.Equal("Evento adiado", card.InvalidationReason);
	}

	private static int[,] CreateCard() => new[,]
	{
		{ 1, 16, 31, 46, 61 },
		{ 2, 17, 32, 47, 62 },
		{ 3, 18, 0, 48, 63 },
		{ 4, 19, 34, 49, 64 },
		{ 5, 20, 35, 50, 65 }
	};
}
