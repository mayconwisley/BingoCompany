using System.Text.Json;
using BingoCompany.Api.Controllers;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class PublicControllerRoundReadinessTests
{
	[Fact]
	public async Task Includes_the_actual_eligible_card_count_for_a_ready_round()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		bingoEvent.OpenRegistration();
		var participant = new Participant(bingoEvent.Id, "Ana");
		var eligibleCard = new BingoCard(bingoEvent.Id, participant.Id, CardType.Digital, CreateCard());
		var ineligibleCard = new BingoCard(bingoEvent.Id, null, CardType.Digital, CreateCard());
		var round = new BingoRound(bingoEvent.Id, 1, "Rodada 1");
		round.AddStage(new PrizeStage(round.Id, 1, "Linha", WinningPattern.HorizontalLine));
		db.Events.Add(bingoEvent);
		db.Participants.Add(participant);
		db.Cards.AddRange(eligibleCard, ineligibleCard);
		db.Rounds.Add(round);
		await db.SaveChangesAsync();
		var controller = new PublicController(
			new BingoCompany.Application.Services.EventParticipantRegistrationService(new BingoCompany.Infrastructure.Persistence.Repositories.EventParticipantRegistrationRepository(db)),
			new BingoCompany.Application.Services.PublicEventQueryService(new BingoCompany.Infrastructure.Persistence.Repositories.PublicEventReadRepository(db)),
			new BingoCompany.Application.Services.PublicCardActivationService(new BingoCompany.Infrastructure.Persistence.Repositories.PublicCardActivationRepository(db)),
			new BingoCompany.Application.Services.PublicEventAuditQueryService(new BingoCompany.Infrastructure.Persistence.Repositories.PublicEventAuditReadRepository(db)));

		var response = Assert.IsType<OkObjectResult>((await controller.Get(bingoEvent.PublicCode)).Result);
		using var stateJson = JsonDocument.Parse(JsonSerializer.Serialize(response.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

		Assert.Equal(1, stateJson.RootElement.GetProperty("round").GetProperty("eligibleCards").GetInt32());
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
