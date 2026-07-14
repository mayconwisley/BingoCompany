using BingoCompany.Application;
using BingoCompany.Application.Services;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;

namespace BingoCompany.Tests;

public sealed class BingoRoundGameplayServiceTests
{
	[Fact]
	public void Draw_detects_eligible_winning_cards_and_starts_tie_breaker()
	{
		var eventId = Guid.CreateVersion7();
		var round = CreateStartedRound(eventId, WinningPattern.BColumn);
		var firstCard = new BingoCard(eventId, Guid.CreateVersion7(), CardType.Digital, CreateCard());
		var secondCard = new BingoCard(eventId, Guid.CreateVersion7(), CardType.Digital, CreateCard());
		var service = new BingoRoundGameplayService();

		RoundDrawResult result = null!;
		for (var index = 0; index < 5; index++)
		{
			result = service.Draw(round, [firstCard, secondCard], [], CardMarkingMode.Automatic, new HashSet<Guid>());
		}

		Assert.Equal(5, result.DrawnNumber.Number);
		Assert.True(result.HasWinners);
		Assert.True(result.RequiresTieBreaker);
		Assert.Equal(2, result.Winners.Count);
		Assert.Equal(RoundStatus.TieBreaker, round.Status);
	}

	[Fact]
	public void Reveal_winner_confirms_one_candidate_without_advancing_the_prize_stage_until_delivery()
	{
		var eventId = Guid.CreateVersion7();
		var round = CreateStartedRound(eventId, WinningPattern.BColumn);
		var firstCandidate = new RoundWinner(round.Id, round.ActiveStage.Id, Guid.CreateVersion7(), Guid.CreateVersion7(), 5);
		var secondCandidate = new RoundWinner(round.Id, round.ActiveStage.Id, Guid.CreateVersion7(), Guid.CreateVersion7(), 5);
		round.DetectWinner();
		round.StartTieBreaker();

		var result = new BingoRoundGameplayService().RevealWinner(round, [firstCandidate, secondCandidate], DateTimeOffset.UtcNow);

		Assert.True(result.TieBreakerApplied);
		Assert.True(result.Winner.IsWinner);
		Assert.NotNull(result.Winner.RevealedAt);
		Assert.Equal(RoundStatus.TieBreaker, round.Status);
		Assert.False(round.Stages.Single().IsCompleted);
	}

	[Fact]
	public void Declined_prize_resumes_the_same_prize_stage()
	{
		var round = CreateStartedRound(Guid.CreateVersion7(), WinningPattern.BColumn);
		var winner = new RoundWinner(round.Id, round.ActiveStage.Id, Guid.CreateVersion7(), Guid.CreateVersion7(), 5);
		round.DetectWinner();
		winner.Confirm(DateTimeOffset.UtcNow);

		winner.MarkPrizeDeclined(DateTimeOffset.UtcNow);
		round.ResumeDrawingAfterPrizeDeclined();

		Assert.NotNull(winner.PrizeDeclinedAt);
		Assert.Equal(RoundStatus.Drawing, round.Status);
		Assert.True(round.ActiveStage.IsActive);
		Assert.False(round.ActiveStage.IsCompleted);
	}

	[Fact]
	public void Delivered_prize_advances_the_prize_stage()
	{
		var round = CreateStartedRound(Guid.CreateVersion7(), WinningPattern.BColumn);
		var winner = new RoundWinner(round.Id, round.ActiveStage.Id, Guid.CreateVersion7(), Guid.CreateVersion7(), 5);
		round.DetectWinner();
		winner.Confirm(DateTimeOffset.UtcNow);

		winner.MarkPrizeDelivered(DateTimeOffset.UtcNow);
		round.FinishStage();

		Assert.NotNull(winner.PrizeDeliveredAt);
		Assert.Equal(RoundStatus.Finished, round.Status);
	}

	[Fact]
	public void Cancel_ends_a_drawing_round_without_completing_its_prize_stage()
	{
		var round = CreateStartedRound(Guid.CreateVersion7(), WinningPattern.HorizontalLine);

		round.Cancel();

		Assert.Equal(RoundStatus.Cancelled, round.Status);
		Assert.False(round.Stages.Single().IsCompleted);
	}

	private static BingoRound CreateStartedRound(Guid eventId, WinningPattern pattern)
	{
		var round = new BingoRound(eventId, 1, "Rodada de teste");
		round.AddStage(new PrizeStage(round.Id, 1, "Prêmio", pattern));
		var sequence = Enumerable.Range(1, 75).ToArray();
		round.Start(sequence, SecureDrawSequence.Hash(sequence));
		return round;
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
