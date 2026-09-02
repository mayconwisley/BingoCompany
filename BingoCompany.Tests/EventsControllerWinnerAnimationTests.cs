using System.Text.Json;
using BingoCompany.Api.Controllers;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class EventsControllerWinnerAnimationTests
{
	[Fact]
	public async Task Hides_the_winner_highlight_when_a_later_round_has_started()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		bingoEvent.OpenRegistration();
		bingoEvent.Start();
		var participant = new Participant(bingoEvent.Id, "Ana");
		var card = new BingoCard(bingoEvent.Id, participant.Id, CardType.Digital, CreateCard());
		var awardedRound = new BingoRound(bingoEvent.Id, 1, "Rodada 1");
		var awardedStage = new PrizeStage(awardedRound.Id, 1, "Vale-presente", WinningPattern.HorizontalLine);
		awardedRound.AddStage(awardedStage);
		awardedRound.Start(Enumerable.Range(1, 75).ToArray(), "hash-1");
		awardedRound.FinishStage();
		var winner = new RoundWinner(awardedRound.Id, awardedStage.Id, card.Id, participant.Id, 1);
		winner.Confirm(DateTimeOffset.UtcNow);
		var nextRound = new BingoRound(bingoEvent.Id, 2, "Rodada 2");
		nextRound.AddStage(new PrizeStage(nextRound.Id, 1, "Prêmio principal", WinningPattern.FullCard));
		nextRound.Start(Enumerable.Range(1, 75).ToArray(), "hash-2");
		db.Events.Add(bingoEvent);
		db.Participants.Add(participant);
		db.Cards.Add(card);
		db.Rounds.AddRange(awardedRound, nextRound);
		db.RoundWinners.Add(winner);
		await db.SaveChangesAsync();
		var controller = new EventsController(null!, null!, null!, null!, null!, new BingoCompany.Application.Services.CardStateQueryService(new BingoCompany.Infrastructure.Persistence.Repositories.CardStateReadRepository(db)), new BingoCompany.Application.Services.CompanyEventsQueryService(new BingoCompany.Infrastructure.Persistence.Repositories.CompanyEventsReadRepository(db)), new BingoCompany.Application.Services.EventConfigurationService(new BingoCompany.Infrastructure.Persistence.Repositories.EventConfigurationRepository(db)), new BingoCompany.Application.Services.CardLifecycleService(new BingoCompany.Infrastructure.Persistence.Repositories.CardLifecycleRepository(db)), new BingoCompany.Application.Services.EventRoundManagementService(new BingoCompany.Infrastructure.Persistence.Repositories.EventRoundManagementRepository(db)));

		var response = Assert.IsType<OkObjectResult>((await controller.CardState(bingoEvent.Id, card.PublicCode)).Result);
		using var stateJson = JsonDocument.Parse(JsonSerializer.Serialize(response.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

		Assert.False(stateJson.RootElement.GetProperty("isWinner").GetBoolean());
	}

	[Fact]
	public async Task Hides_the_winner_highlight_when_the_prize_is_declined()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		bingoEvent.OpenRegistration();
		bingoEvent.Start();
		var participant = new Participant(bingoEvent.Id, "Ana");
		var card = new BingoCard(bingoEvent.Id, participant.Id, CardType.Digital, CreateCard());
		var round = new BingoRound(bingoEvent.Id, 1, "Rodada 1");
		var stage = new PrizeStage(round.Id, 1, "Vale-presente", WinningPattern.HorizontalLine);
		round.AddStage(stage);
		round.Start(Enumerable.Range(1, 75).ToArray(), "hash-1");
		var winner = new RoundWinner(round.Id, stage.Id, card.Id, participant.Id, 1);
		winner.Confirm(DateTimeOffset.UtcNow);
		winner.MarkPrizeDeclined(DateTimeOffset.UtcNow);
		round.DetectWinner();
		round.ResumeDrawingAfterPrizeDeclined();
		db.Events.Add(bingoEvent);
		db.Participants.Add(participant);
		db.Cards.Add(card);
		db.Rounds.Add(round);
		db.RoundWinners.Add(winner);
		await db.SaveChangesAsync();
		var controller = new EventsController(null!, null!, null!, null!, null!, new BingoCompany.Application.Services.CardStateQueryService(new BingoCompany.Infrastructure.Persistence.Repositories.CardStateReadRepository(db)), new BingoCompany.Application.Services.CompanyEventsQueryService(new BingoCompany.Infrastructure.Persistence.Repositories.CompanyEventsReadRepository(db)), new BingoCompany.Application.Services.EventConfigurationService(new BingoCompany.Infrastructure.Persistence.Repositories.EventConfigurationRepository(db)), new BingoCompany.Application.Services.CardLifecycleService(new BingoCompany.Infrastructure.Persistence.Repositories.CardLifecycleRepository(db)), new BingoCompany.Application.Services.EventRoundManagementService(new BingoCompany.Infrastructure.Persistence.Repositories.EventRoundManagementRepository(db)));

		var response = Assert.IsType<OkObjectResult>((await controller.CardState(bingoEvent.Id, card.PublicCode)).Result);
		using var stateJson = JsonDocument.Parse(JsonSerializer.Serialize(response.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

		Assert.False(stateJson.RootElement.GetProperty("isWinner").GetBoolean());
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
