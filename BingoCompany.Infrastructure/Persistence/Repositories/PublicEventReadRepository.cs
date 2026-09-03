using BingoCompany.Application;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Queries;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class PublicEventReadRepository(BingoDbContext db) : IPublicEventReadRepository
{
	public async Task<PublicEventQueryResult?> Get(string publicCode, CancellationToken cancellationToken)
	{
		var bingoEvent = await db.Events
			.Include(item => item.Rounds).ThenInclude(item => item.Stages)
			.Include(item => item.Rounds).ThenInclude(item => item.DrawnNumbers)
			.SingleOrDefaultAsync(item => item.PublicCode == publicCode, cancellationToken);
		if (bingoEvent is null) return null;

		var round = SelectRound(bingoEvent);
		var activeStage = round?.Stages.SingleOrDefault(item => item.IsActive);
		var pendingPrizeWinner = round is null || activeStage is null
			? null
			: SelectWinner(await db.RoundWinners.Where(item => item.RoundId == round.Id && item.StageId == activeStage.Id && item.IsWinner && item.RevealedAt.HasValue && !item.PrizeDeliveredAt.HasValue && !item.PrizeDeclinedAt.HasValue).ToListAsync(cancellationToken));
		var presentationStage = pendingPrizeWinner is null
			? round?.Stages.OrderByDescending(item => item.Sequence).FirstOrDefault(item => item.IsCompleted && !item.IsWinnerPresentationClosed)
			: round!.Stages.Single(item => item.Id == pendingPrizeWinner.StageId);
		var presentationWinners = presentationStage is null ? [] : await db.RoundWinners.Where(item => item.RoundId == round!.Id && item.StageId == presentationStage.Id).ToListAsync(cancellationToken);
		var winner = pendingPrizeWinner ?? SelectWinner(presentationWinners);
		var participantNames = presentationWinners.Count == 0
			? new Dictionary<Guid, string>()
			: await db.Participants.Where(item => presentationWinners.Select(candidate => candidate.ParticipantId).Contains(item.Id)).ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
		var cardCodes = presentationWinners.Count == 0
			? new Dictionary<Guid, string>()
			: await db.Cards.Where(item => presentationWinners.Select(candidate => candidate.CardId).Contains(item.Id)).ToDictionaryAsync(item => item.Id, item => item.PublicCode, cancellationToken);
		var statistics = await CalculateStatistics(round, activeStage, bingoEvent.MarkingMode, cancellationToken);
		var eligibleCards = round?.Status == RoundStatus.Ready
			? await db.Cards.CountAsync(item => item.EventId == bingoEvent.Id && item.Status == CardStatus.Active && item.ParticipantId.HasValue, cancellationToken)
			: round is null ? 0 : await db.RoundEligibleCards.CountAsync(item => item.RoundId == round.Id, cancellationToken);
		var purchasedCards = bingoEvent.CardPurchaseLimit.HasValue
			? await db.PurchasedDigitalCards(bingoEvent.Id).CountAsync(cancellationToken)
			: 0;
		var waitlistEntries = bingoEvent.CardPurchaseLimit.HasValue
			? await db.CardPurchaseWaitlistEntries.CountAsync(item => item.EventId == bingoEvent.Id, cancellationToken)
			: 0;

		var roundResult = round is null
			? null
			: new PublicRoundQueryResult(
				round.Id, round.Name, round.Sequence, round.Status, round.SequenceHash, activeStage?.PrizeName, activeStage?.PrizeImageDataUrl, presentationStage?.PrizeImageDataUrl, eligibleCards,
				round.Stages.OrderBy(item => item.Sequence).Select(item => new PublicPrizeStageQueryResult(item.PrizeName, item.Pattern, item.PrizeImageDataUrl, item.IsActive, item.IsCompleted)).ToArray(),
				round.DrawnNumbers.OrderBy(item => item.Sequence).Select(item => item.Number).ToArray(),
				activeStage is null || round.Status == RoundStatus.Drawing ? 0 : await db.RoundWinners.CountAsync(item => item.RoundId == round.Id && item.StageId == activeStage.Id, cancellationToken),
				round.Status == RoundStatus.TieBreaker,
				pendingPrizeWinner is not null,
				statistics is null ? null : new PublicRoundStatisticsQueryResult(statistics.TotalCards, statistics.OneNumberAway, statistics.TwoNumbersAway, statistics.ThreeNumbersAway, statistics.AwardedCards),
				winner is null ? null : new PublicWinnerQueryResult(participantNames.GetValueOrDefault(winner.ParticipantId, "Participante indisponível"), presentationStage!.PrizeName, presentationStage.Pattern, presentationStage.PrizeImageDataUrl, pendingPrizeWinner is not null, presentationWinners.Count > 1 ? presentationWinners.OrderByDescending(item => item.TieBreakerNumber).Select(item => new PublicTieBreakerQueryResult(participantNames.GetValueOrDefault(item.ParticipantId, "Participante indisponível"), cardCodes.GetValueOrDefault(item.CardId, "Cartela indisponível"), item.TieBreakerNumber, item.Id == winner.Id)).ToArray() : []));

		return new PublicEventQueryResult(bingoEvent.Id, bingoEvent.Name, bingoEvent.PublicCode, bingoEvent.Status, bingoEvent.MarkingMode, bingoEvent.IsCardPurchaseAvailableAt(DateTimeOffset.UtcNow), bingoEvent.CardPurchaseLimit, bingoEvent.CardPurchasePerParticipantLimit, bingoEvent.CardPurchaseClosesAt, bingoEvent.CardPurchaseLowStockThreshold, bingoEvent.CardPurchaseCancellationReason, bingoEvent.CardPurchaseLimit.HasValue ? Math.Max(0, bingoEvent.CardPurchaseLimit.Value - purchasedCards) : null, waitlistEntries, await db.Participants.CountAsync(item => item.EventId == bingoEvent.Id, cancellationToken), await db.Cards.CountAsync(item => item.EventId == bingoEvent.Id, cancellationToken), roundResult);
	}

	private async Task<PublicRoundStatisticsQueryResult?> CalculateStatistics(BingoRound? round, PrizeStage? stage, CardMarkingMode markingMode, CancellationToken cancellationToken)
	{
		if (round is null || stage is null) return null;
		var cardIds = await db.RoundEligibleCards.Where(item => item.RoundId == round.Id).Select(item => item.CardId).ToArrayAsync(cancellationToken);
		var cards = await db.Cards.Where(item => cardIds.Contains(item.Id)).ToArrayAsync(cancellationToken);
		var drawnNumbers = round.DrawnNumbers.Select(item => item.Number).ToHashSet();
		var marks = markingMode == CardMarkingMode.Automatic ? new Dictionary<Guid, IReadOnlySet<int>>() : (await db.CardMarks.Where(item => item.RoundId == round.Id).ToListAsync(cancellationToken)).GroupBy(item => item.CardId).ToDictionary(group => group.Key, group => (IReadOnlySet<int>)group.Select(item => item.Number).ToHashSet());
		var statistics = BingoRoundStatisticsCalculator.Calculate(cards, card => markingMode == CardMarkingMode.Automatic ? drawnNumbers : marks.GetValueOrDefault(card.Id, new HashSet<int>()), stage.Pattern);
		return new PublicRoundStatisticsQueryResult(statistics.TotalCards, statistics.OneNumberAway, statistics.TwoNumbersAway, statistics.ThreeNumbersAway, statistics.AwardedCards);
	}

	private static BingoRound? SelectRound(BingoEvent bingoEvent) => bingoEvent.Rounds.Where(item => item.Status is RoundStatus.Drawing or RoundStatus.WinnerDetected or RoundStatus.TieBreaker).OrderByDescending(item => item.Sequence).FirstOrDefault() ?? bingoEvent.Rounds.Where(item => item.Status == RoundStatus.Ready).OrderBy(item => item.Sequence).FirstOrDefault() ?? bingoEvent.Rounds.OrderByDescending(item => item.Sequence).FirstOrDefault();

	private static RoundWinner? SelectWinner(IEnumerable<RoundWinner> candidates) => candidates.Where(item => item.IsWinner).OrderByDescending(item => item.TieBreakerNumber).ThenBy(item => item.RevealedAt).ThenBy(item => item.Id).FirstOrDefault();
}
