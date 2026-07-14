using BingoCompany.Api.Controllers;
using BingoCompany.Api.Contracts;
using BingoCompany.Domain;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class EventsControllerManualMarkingTests
{
	[Fact]
	public async Task Rejects_a_previous_drawn_number_in_mandatory_manual_marking()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa", markingMode: CardMarkingMode.ManualRequired);
		bingoEvent.OpenRegistration();
		bingoEvent.Start();
		var participant = new Participant(bingoEvent.Id, "Ana");
		var card = new BingoCard(bingoEvent.Id, participant.Id, CardType.Digital, CreateCard());
		var round = new BingoRound(bingoEvent.Id, 1, "Rodada 1");
		round.AddStage(new PrizeStage(round.Id, 1, "Linha", WinningPattern.HorizontalLine));
		round.Start(Enumerable.Range(1, 75).ToArray(), "hash");
		round.DrawNext();
		round.DrawNext();
		db.Events.Add(bingoEvent);
		db.Participants.Add(participant);
		db.Cards.Add(card);
		db.Rounds.Add(round);
		await db.SaveChangesAsync();
		var controller = new EventsController(db, null!, null!, null!);

		var response = await controller.Mark(bingoEvent.Id, card.PublicCode, new MarkNumberRequest(1));

		var conflict = Assert.IsType<ConflictObjectResult>(response);
		Assert.Equal("Na marcação manual obrigatória, somente a pedra atual pode ser marcada.", conflict.Value);
		Assert.Empty(await db.CardMarks.ToListAsync());
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
