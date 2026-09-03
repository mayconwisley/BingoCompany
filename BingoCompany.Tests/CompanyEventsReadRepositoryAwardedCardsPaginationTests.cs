using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Infrastructure.Persistence;
using BingoCompany.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class CompanyEventsReadRepositoryAwardedCardsPaginationTests
{
	[Fact]
	public async Task Get_ReturnsOnlyTheRequestedPageOfAwardedCards()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Evento encerrado");
		bingoEvent.OpenRegistration();
		bingoEvent.Start();

		for (var sequence = 1; sequence <= 4; sequence++)
		{
			var participant = new Participant(bingoEvent.Id, $"Participante {sequence}");
			var card = new BingoCard(bingoEvent.Id, participant.Id, CardType.Digital, CreateCard());
			var round = new BingoRound(bingoEvent.Id, sequence, $"Rodada {sequence}");
			var stage = new PrizeStage(round.Id, 1, $"Prêmio {sequence}", WinningPattern.HorizontalLine);
			round.AddStage(stage);
			round.Start(Enumerable.Range(1, 75).ToArray(), $"hash-{sequence}");
			round.FinishStage();
			var winner = new RoundWinner(round.Id, stage.Id, card.Id, participant.Id, 1);
			winner.Confirm(DateTimeOffset.UtcNow.AddMinutes(sequence));

			bingoEvent.AddParticipant(participant);
			bingoEvent.AddCard(card);
			bingoEvent.AddRound(round);
			db.RoundWinners.Add(winner);
		}

		bingoEvent.Finish();
		db.Events.Add(bingoEvent);
		await db.SaveChangesAsync();
		var repository = new CompanyEventsReadRepository(db);

		var result = await repository.Get(bingoEvent.Id, 2, 3, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(2, result.AwardedCards.Page);
		Assert.Equal(3, result.AwardedCards.PageSize);
		Assert.Equal(4, result.AwardedCards.TotalItems);
		Assert.Equal(2, result.AwardedCards.TotalPages);
		var awardedCard = Assert.Single(result.AwardedCards.Items);
		Assert.Equal("Rodada 4", awardedCard.RoundName);
		Assert.Equal("Prêmio 4", awardedCard.PrizeName);
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
