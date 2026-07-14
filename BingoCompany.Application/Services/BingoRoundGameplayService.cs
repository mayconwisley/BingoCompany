using BingoCompany.Domain;
using BingoCompany.Application.Interfaces;
using BingoCompany.Domain.Entities;

namespace BingoCompany.Application.Services;

public sealed class BingoRoundGameplayService : IBingoRoundGameplayService
{
	public RoundDrawResult Draw(BingoRound round, IReadOnlyCollection<BingoCard> eligibleCards, IReadOnlyCollection<CardMark> marks, CardMarkingMode markingMode)
	{
		var drawnNumber = round.DrawNext();
		var winners = eligibleCards
			.Where(card => IsWinningCard(round, card, marks, markingMode))
			.Select(card => new RoundWinner(round.Id, round.ActiveStage.Id, card.Id, card.ParticipantId!.Value, drawnNumber.Sequence))
			.ToArray();

		if (winners.Length > 0)
		{
			round.DetectWinner();
			if (winners.Length > 1)
			{
				round.StartTieBreaker();
			}
		}

		return new RoundDrawResult(drawnNumber, winners);
	}

	public RoundWinnerDetection DetectManualWinner(BingoRound round, BingoCard card, IReadOnlySet<int> markedNumbers, int drawSequence)
	{
		if (!WinningPatternEvaluator.IsCompleted(card.Numbers, markedNumbers, round.ActiveStage.Pattern))
		{
			return new RoundWinnerDetection(null);
		}

		round.DetectWinner();
		return new RoundWinnerDetection(new RoundWinner(round.Id, round.ActiveStage.Id, card.Id, card.ParticipantId!.Value, drawSequence));
	}

	public WinnerRevealResult RevealWinner(BingoRound round, IReadOnlyCollection<RoundWinner> candidates, DateTimeOffset revealedAt)
	{
		if (candidates.Count == 0)
		{
			throw new InvalidOperationException("Não há candidatos a vencedor para revelar.");
		}

		var tieBreakerApplied = candidates.Count > 1;
		if (tieBreakerApplied)
		{
			var tieBreakers = SecureDrawSequence.Generate().Take(candidates.Count).ToArray();
			for (var index = 0; index < candidates.Count; index++)
			{
				candidates.ElementAt(index).AssignTieBreaker(tieBreakers[index]);
			}
		}

		var winner = candidates.OrderByDescending(candidate => candidate.TieBreakerNumber ?? 0).First();
		winner.Confirm(revealedAt);
		round.FinishStage();
		return new WinnerRevealResult(winner, tieBreakerApplied);
	}

	private static bool IsWinningCard(BingoRound round, BingoCard card, IReadOnlyCollection<CardMark> marks, CardMarkingMode markingMode)
	{
		var markedNumbers = markingMode == CardMarkingMode.Automatic
			? round.DrawnNumbers.Select(item => item.Number).ToHashSet()
			: marks.Where(mark => mark.CardId == card.Id).Select(mark => mark.Number).ToHashSet();

		return WinningPatternEvaluator.IsCompleted(card.Numbers, markedNumbers, round.ActiveStage.Pattern);
	}
}
