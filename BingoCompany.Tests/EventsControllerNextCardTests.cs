using System.Text.Json;
using BingoCompany.Api.Controllers;
using BingoCompany.Domain;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class EventsControllerNextCardTests
{
	[Fact]
	public async Task Allows_an_incomplete_card_from_a_finished_round_to_be_replaced_before_the_next_round()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		bingoEvent.OpenRegistration();
		bingoEvent.Start();
		var participant = new Participant(bingoEvent.Id, "Ana", ParticipantType.FamilyMember, responsibleEmployeeName: "Bruno");
		var card = new BingoCard(bingoEvent.Id, participant.Id, CardType.Digital, CreateCard());
		var round = new BingoRound(bingoEvent.Id, 1, "Rodada 1");
		round.AddStage(new PrizeStage(round.Id, 1, "Linha", WinningPattern.HorizontalLine));
		round.Start(Enumerable.Range(1, 75).ToArray(), "hash");
		round.FinishStage();
		db.Events.Add(bingoEvent);
		db.Participants.Add(participant);
		db.Cards.Add(card);
		db.Rounds.Add(round);
		db.RoundEligibleCards.Add(new RoundEligibleCard(round.Id, card.Id, participant.Id));
		await db.SaveChangesAsync();
		var controller = new EventsController(db, null!, null!, null!);

		var state = Assert.IsType<OkObjectResult>((await controller.CardState(bingoEvent.Id, card.PublicCode)).Result);
		using var stateJson = JsonDocument.Parse(JsonSerializer.Serialize(state.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
		Assert.True(stateJson.RootElement.GetProperty("canGenerateNextCard").GetBoolean());
		Assert.Equal("Ana", stateJson.RootElement.GetProperty("participantName").GetString());
		Assert.Equal("Bruno", stateJson.RootElement.GetProperty("responsibleEmployeeName").GetString());

		var response = Assert.IsType<OkObjectResult>((await controller.GenerateNextCard(bingoEvent.Id, card.PublicCode)).Result);
		Assert.NotNull(response.Value);
		Assert.True(card.ReplacementCardId.HasValue);
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
