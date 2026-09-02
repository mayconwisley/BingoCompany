using BingoCompany.Api.Controllers;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class EventsControllerStartRoundTests
{
	[Fact]
	public async Task Rejects_starting_a_round_without_an_active_participant_card()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		bingoEvent.OpenRegistration();
		var round = new BingoRound(bingoEvent.Id, 1, "Rodada 1");
		round.AddStage(new PrizeStage(round.Id, 1, "Vale-presente", WinningPattern.HorizontalLine));
		db.Events.Add(bingoEvent);
		db.Rounds.Add(round);
		await db.SaveChangesAsync();
		var controller = new EventsController(null!, null!, null!, null!, null!, new BingoCompany.Application.Services.CardStateQueryService(new BingoCompany.Infrastructure.Persistence.Repositories.CardStateReadRepository(db)), new BingoCompany.Application.Services.CompanyEventsQueryService(new BingoCompany.Infrastructure.Persistence.Repositories.CompanyEventsReadRepository(db)), new BingoCompany.Application.Services.EventConfigurationService(new BingoCompany.Infrastructure.Persistence.Repositories.EventConfigurationRepository(db)), new BingoCompany.Application.Services.CardLifecycleService(new BingoCompany.Infrastructure.Persistence.Repositories.CardLifecycleRepository(db)), new BingoCompany.Application.Services.EventRoundManagementService(new BingoCompany.Infrastructure.Persistence.Repositories.EventRoundManagementRepository(db)));

		var response = await controller.StartRound(bingoEvent.Id, round.Id);

		var conflict = Assert.IsType<ConflictObjectResult>(response);
		Assert.Equal("Gere e ative ao menos uma cartela associada a participante antes de abrir a operação.", conflict.Value);
		Assert.Equal(EventStatus.RegistrationOpen, bingoEvent.Status);
	}
}
