using BingoCompany.Api.Controllers;
using BingoCompany.Api.Contracts;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class EventsControllerUpdateRoundTests
{
	[Fact]
	public async Task Replaces_prize_stages_without_deleting_the_same_stage_twice()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		var round = new BingoRound(bingoEvent.Id, 1, "Rodada original");
		round.AddStage(new PrizeStage(round.Id, 1, "Vale-presente", WinningPattern.HorizontalLine));

		await using (var setupDb = new BingoDbContext(options))
		{
			setupDb.Events.Add(bingoEvent);
			setupDb.Rounds.Add(round);
			await setupDb.SaveChangesAsync();
		}

		await using var db = new BingoDbContext(options);
		var controller = new EventsController(db, null!, null!, null!);
		var request = new CreateRoundRequest("Rodada atualizada", [
			new CreatePrizeStageRequest(1, "Vale-presente", WinningPattern.HorizontalLine),
			new CreatePrizeStageRequest(2, "Prêmio principal", WinningPattern.FullCard)
		]);

		var response = await controller.UpdateRound(bingoEvent.Id, round.Id, request);

		Assert.IsType<NoContentResult>(response);
		var stages = await db.PrizeStages.Where(item => item.RoundId == round.Id).OrderBy(item => item.Sequence).ToListAsync();
		Assert.Equal(["Vale-presente", "Prêmio principal"], stages.Select(item => item.PrizeName));
	}
}
