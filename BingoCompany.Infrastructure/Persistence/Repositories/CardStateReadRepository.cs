using BingoCompany.Application;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Queries;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class CardStateReadRepository(BingoDbContext db) : ICardStateReadRepository
{
	public async Task<CardStateSnapshot?> Get(Guid eventId, string cardCode, CancellationToken cancellationToken)
	{
		var card = await db.Cards.AsNoTracking().SingleOrDefaultAsync(item => item.EventId == eventId && item.PublicCode == cardCode, cancellationToken);
		if (card is null) return null;

		var bingoEvent = await db.Events.AsNoTracking().SingleAsync(item => item.Id == eventId, cancellationToken);
		var round = await db.Rounds
			.Include(item => item.Stages)
			.Include(item => item.DrawnNumbers)
			.SingleOrDefaultAsync(
				item => item.EventId == eventId && (item.Status == RoundStatus.Drawing || item.Status == RoundStatus.WinnerDetected || item.Status == RoundStatus.TieBreaker),
				cancellationToken);
		var participant = card.Type == CardType.Digital && card.ParticipantId.HasValue
			? await db.Participants.AsNoTracking().SingleOrDefaultAsync(item => item.Id == card.ParticipantId.Value && item.EventId == eventId, cancellationToken)
			: null;
		var marks = round is null
			? []
			: await db.CardMarks.Where(item => item.RoundId == round.Id && item.CardId == card.Id).OrderBy(item => item.MarkedAt).Select(item => item.Number).ToArrayAsync(cancellationToken);
		var previousRound = await db.Rounds
			.Where(item => item.EventId == eventId && (item.Status == RoundStatus.Finished || item.Status == RoundStatus.Cancelled))
			.OrderByDescending(item => item.Sequence)
			.FirstOrDefaultAsync(cancellationToken);
		var canGenerateNextCard = previousRound is not null
			&& bingoEvent.Status != EventStatus.Finished
			&& !card.ReplacementCardId.HasValue
			&& !await db.Rounds.AnyAsync(item => item.EventId == eventId && item.Sequence > previousRound.Sequence && (item.Status == RoundStatus.Drawing || item.Status == RoundStatus.WinnerDetected || item.Status == RoundStatus.TieBreaker), cancellationToken)
			&& await db.RoundEligibleCards.AnyAsync(item => item.RoundId == previousRound.Id && item.CardId == card.Id, cancellationToken);
		var currentRoundSequence = round?.Sequence;
		var isWinner = await db.RoundWinners
			.Join(db.PrizeStages, winner => winner.StageId, stage => stage.Id, (winner, stage) => new { winner, stage })
			.Join(db.Rounds, item => item.winner.RoundId, winnerRound => winnerRound.Id, (item, winnerRound) => new { item.winner, item.stage, winnerRound })
			.AnyAsync(
				item => item.winner.CardId == card.Id && item.winner.IsWinner && item.winner.RevealedAt.HasValue && !item.winner.PrizeDeclinedAt.HasValue && !item.stage.IsWinnerPresentationClosed && (!currentRoundSequence.HasValue || item.winnerRound.Sequence >= currentRoundSequence.Value),
				cancellationToken);
		var activeStage = round?.Stages.SingleOrDefault(item => item.IsActive);
		var progressMarkedNumbers = bingoEvent.MarkingMode == CardMarkingMode.Automatic
			? round?.DrawnNumbers.Select(item => item.Number).ToHashSet() ?? []
			: marks.ToHashSet();
		var remainingNumbersToWin = activeStage is null ? (int?)null : WinningPatternEvaluator.RemainingNumbers(card.Numbers, progressMarkedNumbers, activeStage.Pattern);

		return new CardStateSnapshot(
			card.Id,
			card.PublicCode,
			participant?.Name,
			participant?.ResponsibleEmployeeName,
			isWinner,
			ToRows(card.Numbers),
			bingoEvent.Status,
			bingoEvent.MarkingMode,
			round?.Id,
			round?.Status,
			activeStage?.PrizeName,
			activeStage?.Pattern,
			remainingNumbersToWin,
			round?.DrawnNumbers.OrderBy(item => item.Sequence).Select(item => item.Number).ToArray() ?? [],
			marks,
			round?.DrawnNumbers.Count ?? 0,
			canGenerateNextCard);
	}

	private static int[][] ToRows(int[,] card) => Enumerable.Range(0, 5).Select(row => Enumerable.Range(0, 5).Select(column => card[row, column]).ToArray()).ToArray();
}
