using System.Text.Json;
using BingoCompany.Api.Controllers;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class PublicControllerAuditPaginationTests
{
	[Fact]
	public async Task Audit_Returns_only_the_requested_page_of_entries()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		db.Events.Add(bingoEvent);
		db.AuditEntries.AddRange(
			new AuditEntry(bingoEvent.Id, "Ação 1", "Detalhes 1"),
			new AuditEntry(bingoEvent.Id, "Ação 2", "Detalhes 2"),
			new AuditEntry(bingoEvent.Id, "Ação 3", "Detalhes 3"),
			new AuditEntry(bingoEvent.Id, "Ação 4", "Detalhes 4"),
			new AuditEntry(bingoEvent.Id, "Ação 5", "Detalhes 5"));
		await db.SaveChangesAsync();
		var controller = CreateController(db);

		var response = Assert.IsType<OkObjectResult>((await controller.Audit(bingoEvent.PublicCode, page: 2, pageSize: 2)).Result);
		using var document = JsonDocument.Parse(JsonSerializer.Serialize(response.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
		var entries = document.RootElement.GetProperty("entries");

		Assert.Equal(2, entries.GetProperty("page").GetInt32());
		Assert.Equal(2, entries.GetProperty("pageSize").GetInt32());
		Assert.Equal(5, entries.GetProperty("totalItems").GetInt32());
		Assert.Equal(3, entries.GetProperty("totalPages").GetInt32());
		Assert.Equal(2, entries.GetProperty("items").GetArrayLength());
	}

	[Fact]
	public async Task Audit_Returns_the_card_code_for_each_winner_or_tied_candidate()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		var round = new BingoRound(bingoEvent.Id, 1, "Rodada 1");
		var stage = new PrizeStage(round.Id, 1, "Prêmio", WinningPattern.HorizontalLine);
		var participant = new Participant(bingoEvent.Id, "Mesmo nome");
		var card = new BingoCard(bingoEvent.Id, participant.Id, CardType.Digital, CreateCard());
		var winner = new RoundWinner(round.Id, stage.Id, card.Id, participant.Id, 1);
		round.AddStage(stage);
		db.AddRange(bingoEvent, round, participant, card, winner);
		await db.SaveChangesAsync();
		var controller = CreateController(db);

		var response = Assert.IsType<OkObjectResult>((await controller.Audit(bingoEvent.PublicCode)).Result);
		using var document = JsonDocument.Parse(JsonSerializer.Serialize(response.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
		var auditWinner = document.RootElement.GetProperty("rounds")[0].GetProperty("winners")[0];

		Assert.Equal("Mesmo nome", auditWinner.GetProperty("participantName").GetString());
		Assert.Equal(card.PublicCode, auditWinner.GetProperty("cardCode").GetString());
	}

	private static int[,] CreateCard() => new[,]
	{
		{ 1, 16, 31, 46, 61 },
		{ 2, 17, 32, 47, 62 },
		{ 3, 18, 0, 48, 63 },
		{ 4, 19, 34, 49, 64 },
		{ 5, 20, 35, 50, 65 }
	};

	private static PublicController CreateController(BingoDbContext db) => new(
		new BingoCompany.Application.Services.EventParticipantRegistrationService(new BingoCompany.Infrastructure.Persistence.Repositories.EventParticipantRegistrationRepository(db)),
		new BingoCompany.Application.Services.PublicEventQueryService(new BingoCompany.Infrastructure.Persistence.Repositories.PublicEventReadRepository(db)),
		new BingoCompany.Application.Services.PublicCardActivationService(new BingoCompany.Infrastructure.Persistence.Repositories.PublicCardActivationRepository(db)),
		new BingoCompany.Application.Services.PublicEventAuditQueryService(new BingoCompany.Infrastructure.Persistence.Repositories.PublicEventAuditReadRepository(db)));
}
