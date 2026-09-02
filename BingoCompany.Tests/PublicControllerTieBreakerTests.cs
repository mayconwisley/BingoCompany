using System.Text.Json;
using BingoCompany.Api.Controllers;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class PublicControllerTieBreakerTests
{
	[Fact]
	public async Task Get_WhenMultipleCandidatesWereConfirmed_ReturnsOnlyTheDeterministicTieBreakerWinner()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Evento público");
		var round = new BingoRound(bingoEvent.Id, 1, "Rodada");
		var stage = new PrizeStage(round.Id, 1, "Prêmio", WinningPattern.HorizontalLine);
		round.AddStage(stage);
		round.Start(Enumerable.Range(1, 75).ToArray(), "hash");
		round.DetectWinner();
		round.StartTieBreaker();
		var ana = new Participant(bingoEvent.Id, "Ana");
		var bruno = new Participant(bingoEvent.Id, "Bruno");
		var anaCard = new BingoCard(bingoEvent.Id, ana.Id, CardType.Digital, CreateCard());
		var brunoCard = new BingoCard(bingoEvent.Id, bruno.Id, CardType.Digital, CreateCard());
		var anaWinner = new RoundWinner(round.Id, stage.Id, anaCard.Id, ana.Id, 25);
		anaWinner.AssignTieBreaker(71);
		anaWinner.Confirm(DateTimeOffset.UtcNow);
		var brunoWinner = new RoundWinner(round.Id, stage.Id, brunoCard.Id, bruno.Id, 25);
		brunoWinner.AssignTieBreaker(24);
		brunoWinner.Confirm(DateTimeOffset.UtcNow);
		round.FinishStage();

		db.AddRange(bingoEvent, round, ana, bruno, anaCard, brunoCard, anaWinner, brunoWinner);
		await db.SaveChangesAsync();
		var controller = PublicControllerFactory.Create(db);

		var response = Assert.IsType<OkObjectResult>((await controller.Get(bingoEvent.PublicCode)).Result);
		using var document = JsonDocument.Parse(JsonSerializer.Serialize(response.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
		var winner = document.RootElement.GetProperty("round").GetProperty("winner");
		var tieBreakers = winner.GetProperty("tieBreakers");

		Assert.Equal("Ana", winner.GetProperty("participantName").GetString());
		Assert.Equal(anaCard.PublicCode, tieBreakers[0].GetProperty("cardCode").GetString());
		Assert.Equal(brunoCard.PublicCode, tieBreakers[1].GetProperty("cardCode").GetString());
		Assert.True(tieBreakers[0].GetProperty("isWinner").GetBoolean());
		Assert.False(tieBreakers[1].GetProperty("isWinner").GetBoolean());
	}

	[Fact]
	public async Task Get_WhenTieBreakerIsRevealed_ReturnsAllParticipantsNumbersAndWinner()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Evento público");
		var round = new BingoRound(bingoEvent.Id, 1, "Rodada");
		var stage = new PrizeStage(round.Id, 1, "Prêmio", WinningPattern.HorizontalLine);
		round.AddStage(stage);
		var sequence = Enumerable.Range(1, 75).ToArray();
		round.Start(sequence, "hash");
		round.DetectWinner();
		round.StartTieBreaker();

		var ana = new Participant(bingoEvent.Id, "Ana");
		var bruno = new Participant(bingoEvent.Id, "Bruno");
		var anaCard = new BingoCard(bingoEvent.Id, ana.Id, CardType.Digital, CreateCard());
		var brunoCard = new BingoCard(bingoEvent.Id, bruno.Id, CardType.Digital, CreateCard());
		var anaWinner = new RoundWinner(round.Id, stage.Id, anaCard.Id, ana.Id, 25);
		anaWinner.AssignTieBreaker(71);
		anaWinner.Confirm(DateTimeOffset.UtcNow);
		var brunoWinner = new RoundWinner(round.Id, stage.Id, brunoCard.Id, bruno.Id, 25);
		brunoWinner.AssignTieBreaker(24);
		round.FinishStage();

		db.AddRange(bingoEvent, round, ana, bruno, anaCard, brunoCard, anaWinner, brunoWinner);
		await db.SaveChangesAsync();
		var controller = PublicControllerFactory.Create(db);

		var response = Assert.IsType<OkObjectResult>((await controller.Get(bingoEvent.PublicCode)).Result);
		using var document = JsonDocument.Parse(JsonSerializer.Serialize(response.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
		var tieBreakers = document.RootElement.GetProperty("round").GetProperty("winner").GetProperty("tieBreakers");

		Assert.Equal(2, tieBreakers.GetArrayLength());
		Assert.Equal("Ana", tieBreakers[0].GetProperty("participantName").GetString());
		Assert.Equal(anaCard.PublicCode, tieBreakers[0].GetProperty("cardCode").GetString());
		Assert.Equal(71, tieBreakers[0].GetProperty("number").GetInt32());
		Assert.True(tieBreakers[0].GetProperty("isWinner").GetBoolean());
		Assert.Equal("Bruno", tieBreakers[1].GetProperty("participantName").GetString());
		Assert.Equal(brunoCard.PublicCode, tieBreakers[1].GetProperty("cardCode").GetString());
		Assert.Equal(24, tieBreakers[1].GetProperty("number").GetInt32());
		Assert.False(tieBreakers[1].GetProperty("isWinner").GetBoolean());
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

file static class PublicControllerFactory
{
	public static PublicController Create(BingoDbContext db) => new(
		new BingoCompany.Application.Services.EventParticipantRegistrationService(new BingoCompany.Infrastructure.Persistence.Repositories.EventParticipantRegistrationRepository(db)),
		new BingoCompany.Application.Services.PublicEventQueryService(new BingoCompany.Infrastructure.Persistence.Repositories.PublicEventReadRepository(db)),
		new BingoCompany.Application.Services.PublicCardActivationService(new BingoCompany.Infrastructure.Persistence.Repositories.PublicCardActivationRepository(db)),
		new BingoCompany.Application.Services.PublicEventAuditQueryService(new BingoCompany.Infrastructure.Persistence.Repositories.PublicEventAuditReadRepository(db)));
}
