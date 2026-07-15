using BingoCompany.Api.Interfaces;
using BingoCompany.Application;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/public/events")]
public sealed partial class PublicController(BingoDbContext db, IEventParticipantRegistrationService participantRegistrationService) : ControllerBase
{
	[HttpGet("{code}")]
	public async Task<ActionResult<object>> Get(string code)
	{
		var bingoEvent = await db.Events
			.Include(item => item.Rounds).ThenInclude(item => item.Stages)
			.Include(item => item.Rounds).ThenInclude(item => item.DrawnNumbers)
			.SingleOrDefaultAsync(item => item.PublicCode == code);
		if (bingoEvent is null) return NotFound();

		var round = SelectRound(bingoEvent);
		var activeStage = round?.Stages.SingleOrDefault(item => item.IsActive);
		var pendingPrizeWinner = round is null || activeStage is null
			? null
			: SelectWinner(await db.RoundWinners
				.Where(item => item.RoundId == round.Id && item.StageId == activeStage.Id && item.IsWinner && item.RevealedAt.HasValue && !item.PrizeDeliveredAt.HasValue && !item.PrizeDeclinedAt.HasValue)
				.ToListAsync());
		var presentationStage = pendingPrizeWinner is null
			? round?.Stages.OrderByDescending(item => item.Sequence).FirstOrDefault(item => item.IsCompleted && !item.IsWinnerPresentationClosed)
			: round!.Stages.Single(item => item.Id == pendingPrizeWinner.StageId);
		var presentationWinners = presentationStage is null
			? []
			: await db.RoundWinners.Where(item => item.RoundId == round!.Id && item.StageId == presentationStage.Id).ToListAsync();
		var winner = pendingPrizeWinner ?? SelectWinner(presentationWinners);
		var presentationParticipantNames = presentationWinners.Count == 0
			? new Dictionary<Guid, string>()
			: await db.Participants.Where(item => presentationWinners.Select(winner => winner.ParticipantId).Contains(item.Id)).ToDictionaryAsync(item => item.Id, item => item.Name);
		var winnerName = winner is null ? null : presentationParticipantNames.GetValueOrDefault(winner.ParticipantId, "Participante indisponível");
		var winnerDetectedCount = activeStage is null || round is null || round.Status == RoundStatus.Drawing
			? 0
			: await db.RoundWinners.CountAsync(item => item.RoundId == round.Id && item.StageId == activeStage.Id);
		var statistics = await CalculateStatistics(round, activeStage, bingoEvent.MarkingMode);

		var purchasedCards = bingoEvent.CardPurchaseLimit.HasValue
			? await db.Cards.Join(db.Participants, card => card.ParticipantId, participant => participant.Id, (card, participant) => new { card, participant })
				.CountAsync(item => item.card.EventId == bingoEvent.Id && item.card.Type == CardType.Digital && item.participant.ParticipantAccountId.HasValue)
			: 0;
		var cardPurchaseRemaining = bingoEvent.CardPurchaseLimit.HasValue ? Math.Max(0, bingoEvent.CardPurchaseLimit.Value - purchasedCards) : (int?)null;

		return Ok(new
		{
			bingoEvent.Id,
			bingoEvent.Name,
			bingoEvent.PublicCode,
			bingoEvent.Status,
			bingoEvent.MarkingMode,
			bingoEvent.IsCardPurchaseOpen,
			bingoEvent.CardPurchaseLimit,
			bingoEvent.CardPurchaseCancellationReason,
			cardPurchaseRemaining,
			participants = await db.Participants.CountAsync(item => item.EventId == bingoEvent.Id),
			cards = await db.Cards.CountAsync(item => item.EventId == bingoEvent.Id),
			round = round is null ? null : new
			{
				round.Id,
				round.Name,
				round.Sequence,
				round.Status,
				round.SequenceHash,
				currentPrize = activeStage?.PrizeName,
				currentPrizeImageDataUrl = activeStage?.PrizeImageDataUrl,
				presentationPrizeImageDataUrl = presentationStage?.PrizeImageDataUrl,
				stages = round.Stages.OrderBy(item => item.Sequence).Select(item => new { item.PrizeName, item.Pattern, item.PrizeImageDataUrl, item.IsActive, item.IsCompleted }),
				drawnNumbers = round.DrawnNumbers.OrderBy(item => item.Sequence).Select(item => item.Number),
				winnerDetectedCount,
				tieBreakerRequired = round.Status == RoundStatus.TieBreaker,
				hasPrizeDeliveryPending = pendingPrizeWinner is not null,
				statistics,
				winner = winner is null
					? null
					: new
					{
						participantName = winnerName,
						prizeName = presentationStage!.PrizeName,
						pattern = presentationStage.Pattern,
						prizeImageDataUrl = presentationStage.PrizeImageDataUrl,
						isPrizeDeliveryPending = pendingPrizeWinner is not null,
						tieBreakers = presentationWinners.Count > 1
							? presentationWinners.OrderByDescending(item => item.TieBreakerNumber).Select(item => new { participantName = presentationParticipantNames.GetValueOrDefault(item.ParticipantId, "Participante indisponível"), number = item.TieBreakerNumber, isWinner = item.Id == winner.Id })
							: []
					}
			}
		});
	}

	private static BingoRound? SelectRound(BingoEvent bingoEvent) => bingoEvent.Rounds
		.Where(item => item.Status is RoundStatus.Drawing or RoundStatus.WinnerDetected or RoundStatus.TieBreaker)
		.OrderByDescending(item => item.Sequence)
		.FirstOrDefault()
		?? bingoEvent.Rounds.Where(item => item.Status == RoundStatus.Ready).OrderBy(item => item.Sequence).FirstOrDefault()
		?? bingoEvent.Rounds.OrderByDescending(item => item.Sequence).FirstOrDefault();

	private static RoundWinner? SelectWinner(IEnumerable<RoundWinner> candidates) => candidates
		.Where(item => item.IsWinner)
		.OrderByDescending(item => item.TieBreakerNumber)
		.ThenBy(item => item.RevealedAt)
		.ThenBy(item => item.Id)
		.FirstOrDefault();

	private async Task<object?> CalculateStatistics(BingoRound? round, PrizeStage? stage, CardMarkingMode markingMode)
	{
		if (round is null || stage is null) return null;

		var cardIds = await db.RoundEligibleCards.Where(item => item.RoundId == round.Id).Select(item => item.CardId).ToArrayAsync();
		var cards = await db.Cards.Where(item => cardIds.Contains(item.Id)).ToArrayAsync();
		var drawnNumbers = round.DrawnNumbers.Select(item => item.Number).ToHashSet();
		var marks = markingMode == CardMarkingMode.Automatic
			? new Dictionary<Guid, IReadOnlySet<int>>()
			: (await db.CardMarks.Where(item => item.RoundId == round.Id).ToListAsync()).GroupBy(item => item.CardId).ToDictionary(group => group.Key, group => (IReadOnlySet<int>)group.Select(item => item.Number).ToHashSet());
		var statistics = BingoRoundStatisticsCalculator.Calculate(cards, card => markingMode == CardMarkingMode.Automatic ? drawnNumbers : marks.GetValueOrDefault(card.Id, new HashSet<int>()), stage.Pattern);
		return new { totalCards = statistics.TotalCards, oneNumberAway = statistics.OneNumberAway, twoNumbersAway = statistics.TwoNumbersAway, threeNumbersAway = statistics.ThreeNumbersAway, awardedCards = statistics.AwardedCards };
	}
}
