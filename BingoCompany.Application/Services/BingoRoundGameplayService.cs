using BingoCompany.Domain;
using BingoCompany.Application.Interfaces;
using BingoCompany.Domain.Entities;

namespace BingoCompany.Application.Services;

public sealed class BingoRoundGameplayService : IBingoRoundGameplayService
{
	private static readonly Guid AutomaticMarksKey = Guid.Empty;

	public RoundDrawResult Draw(BingoRound round, IReadOnlyCollection<BingoCard> eligibleCards, IReadOnlyCollection<CardMark> marks, CardMarkingMode markingMode, IReadOnlySet<Guid> excludedCardIds)
	{
		var drawnNumber = round.DrawNext();
		var markedNumbersByCard = GetMarkedNumbersByCard(round, marks, markingMode);
		var winners = eligibleCards
			.Where(card => !excludedCardIds.Contains(card.Id))
			.Where(card => IsWinningCard(round, card, markedNumbersByCard, markingMode))
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

	public RoundWinnerDetection DetectManualWinner(BingoRound round, BingoCard card, IReadOnlySet<int> markedNumbers, int drawSequence, bool isCardExcluded)
	{
		if (isCardExcluded || !WinningPatternEvaluator.IsCompleted(card.Numbers, markedNumbers, round.ActiveStage.Pattern))
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
			var tieBreakers = SecureTieBreakerSequence.Generate(candidates.Count);
			for (var index = 0; index < candidates.Count; index++)
			{
				candidates.ElementAt(index).AssignTieBreaker(tieBreakers[index]);
			}
		}

		var winner = candidates.OrderByDescending(candidate => candidate.TieBreakerNumber ?? 0).First();
		winner.Confirm(revealedAt);
		return new WinnerRevealResult(winner, tieBreakerApplied);
	}

	private static IReadOnlyDictionary<Guid, IReadOnlySet<int>> GetMarkedNumbersByCard(BingoRound round, IReadOnlyCollection<CardMark> marks, CardMarkingMode markingMode)
	{
		if (markingMode == CardMarkingMode.Automatic)
		{
			return new Dictionary<Guid, IReadOnlySet<int>> { [AutomaticMarksKey] = round.DrawnNumbers.Select(item => item.Number).ToHashSet() };
		}

		return marks
			.GroupBy(mark => mark.CardId)
			.ToDictionary(group => group.Key, group => (IReadOnlySet<int>)group.Select(mark => mark.Number).ToHashSet());
	}

	private static bool IsWinningCard(BingoRound round, BingoCard card, IReadOnlyDictionary<Guid, IReadOnlySet<int>> markedNumbersByCard, CardMarkingMode markingMode)
	{
		var markedNumbers = markingMode == CardMarkingMode.Automatic
			? markedNumbersByCard[AutomaticMarksKey]
			: markedNumbersByCard.GetValueOrDefault(card.Id, new HashSet<int>());

		return WinningPatternEvaluator.IsCompleted(card.Numbers, markedNumbers, round.ActiveStage.Pattern);
	}
}
