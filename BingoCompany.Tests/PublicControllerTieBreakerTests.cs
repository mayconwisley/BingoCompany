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
		var anaWinner = new RoundWinner(round.Id, stage.Id, Guid.CreateVersion7(), ana.Id, 25);
		anaWinner.AssignTieBreaker(71);
		anaWinner.Confirm(DateTimeOffset.UtcNow);
		var brunoWinner = new RoundWinner(round.Id, stage.Id, Guid.CreateVersion7(), bruno.Id, 25);
		brunoWinner.AssignTieBreaker(24);
		round.FinishStage();

		db.AddRange(bingoEvent, round, ana, bruno, anaWinner, brunoWinner);
		await db.SaveChangesAsync();
		var controller = new PublicController(db, null!);

		var response = Assert.IsType<OkObjectResult>((await controller.Get(bingoEvent.PublicCode)).Result);
		using var document = JsonDocument.Parse(JsonSerializer.Serialize(response.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
		var tieBreakers = document.RootElement.GetProperty("round").GetProperty("winner").GetProperty("tieBreakers");

		Assert.Equal(2, tieBreakers.GetArrayLength());
		Assert.Equal("Ana", tieBreakers[0].GetProperty("participantName").GetString());
		Assert.Equal(71, tieBreakers[0].GetProperty("number").GetInt32());
		Assert.True(tieBreakers[0].GetProperty("isWinner").GetBoolean());
		Assert.Equal("Bruno", tieBreakers[1].GetProperty("participantName").GetString());
		Assert.Equal(24, tieBreakers[1].GetProperty("number").GetInt32());
		Assert.False(tieBreakers[1].GetProperty("isWinner").GetBoolean());
	}
}
