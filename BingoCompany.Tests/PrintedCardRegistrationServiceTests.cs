using BingoCompany.Application.Services;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Infrastructure.Persistence;
using BingoCompany.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class PrintedCardRegistrationServiceTests
{
	[Fact]
	public async Task Register_creates_participant_and_activates_printed_card()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		var card = new BingoCard(bingoEvent.Id, null, CardType.Printed, CreateCard());
		db.Events.Add(bingoEvent);
		db.Cards.Add(card);
		await db.SaveChangesAsync();
		var service = new PrintedCardRegistrationService(new PrintedCardRegistrationRepository(db));

		var result = await service.Register(bingoEvent.Id, card.PublicCode, new PrintedCardParticipantRegistration("Ana", ParticipantType.Employee, "123", null), CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(CardStatus.Active, card.Status);
		Assert.Equal("Ana", (await db.Participants.SingleAsync()).Name);
		Assert.Equal(card.PublicCode, result.CardCode);
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
