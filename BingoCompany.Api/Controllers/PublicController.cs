using BingoCompany.Application;
using BingoCompany.Api.Contracts;
using BingoCompany.Api.Interfaces;
using BingoCompany.Api.Services;
using BingoCompany.Domain;
using BingoCompany.Infrastructure;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using BingoCompany.Domain.Entities;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/public/events")]
public sealed class PublicController(BingoDbContext db, IEventParticipantRegistrationService participantRegistrationService) : ControllerBase
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
		var pendingPrizeWinner = round is null
			? null
			: await db.RoundWinners.SingleOrDefaultAsync(item => item.RoundId == round.Id && item.IsWinner && item.RevealedAt.HasValue && !item.PrizeDeliveredAt.HasValue && !item.PrizeDeclinedAt.HasValue);
		var presentationStage = pendingPrizeWinner is null
			? round?.Stages.OrderByDescending(item => item.Sequence).FirstOrDefault(item => item.IsCompleted && !item.IsWinnerPresentationClosed)
			: round!.Stages.Single(item => item.Id == pendingPrizeWinner.StageId);
		var presentationWinners = presentationStage is null
			? []
			: await db.RoundWinners.Where(item => item.RoundId == round!.Id && item.StageId == presentationStage.Id).ToListAsync();
		var winner = pendingPrizeWinner ?? presentationWinners.SingleOrDefault(item => item.IsWinner);
		var presentationParticipantNames = presentationWinners.Count == 0
			? new Dictionary<Guid, string>()
			: await db.Participants.Where(item => presentationWinners.Select(winner => winner.ParticipantId).Contains(item.Id)).ToDictionaryAsync(item => item.Id, item => item.Name);
		var winnerName = winner is null ? null : presentationParticipantNames.GetValueOrDefault(winner.ParticipantId, "Participante indisponível");
		var winnerDetectedCount = activeStage is null || round is null || round.Status == RoundStatus.Drawing
			? 0
			: await db.RoundWinners.CountAsync(item => item.RoundId == round.Id && item.StageId == activeStage.Id);
		var statistics = await CalculateStatistics(round, activeStage, bingoEvent.MarkingMode);

		return Ok(new
		{
			bingoEvent.Id,
			bingoEvent.Name,
			bingoEvent.PublicCode,
			bingoEvent.Status,
			bingoEvent.MarkingMode,
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
							? presentationWinners.OrderByDescending(item => item.TieBreakerNumber).Select(item => new { participantName = presentationParticipantNames.GetValueOrDefault(item.ParticipantId, "Participante indisponível"), number = item.TieBreakerNumber, item.IsWinner })
							: []
					}
			}
		});
	}

	[HttpPost("{code}/join")]
	[EnableRateLimiting("public-join")]
	public async Task<ActionResult<object>> Join(string code, JoinEventRequest request)
	{
		var bingoEvent = await db.Events.SingleOrDefaultAsync(item => item.PublicCode == code);
		if (bingoEvent is null) return NotFound();
		try
		{
			var registration = await participantRegistrationService.Register(bingoEvent.Id, request, HttpContext.RequestAborted);
			return registration is null
				? NotFound()
				: Ok(new { participantId = registration.ParticipantId, cardId = registration.CardId, registration.PublicCode, registration.Numbers });
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}

	[HttpGet("{code}/audit")]
	public async Task<ActionResult<object>> Audit(string code)
	{
		var bingoEvent = await db.Events.SingleOrDefaultAsync(item => item.PublicCode == code);
		if (bingoEvent is null) return NotFound();

		var rounds = await db.Rounds.Where(item => item.EventId == bingoEvent.Id).OrderBy(item => item.Sequence).Include(item => item.Stages).Include(item => item.DrawnNumbers).ToListAsync();
		var participants = await db.Participants.Where(item => item.EventId == bingoEvent.Id).OrderBy(item => item.JoinedAt).Select(item => new { item.Id, item.Name, item.JoinedAt }).ToListAsync();
		var cards = await db.Cards.Where(item => item.EventId == bingoEvent.Id).OrderBy(item => item.CreatedAt).Select(item => new { item.Id, item.PublicCode, item.Type, item.Status, item.ParticipantId, item.CreatedAt }).ToListAsync();
		var winners = await db.RoundWinners.Where(item => rounds.Select(round => round.Id).Contains(item.RoundId)).ToListAsync();
		var participantNames = participants.ToDictionary(item => item.Id, item => item.Name);
		var stageNames = rounds.SelectMany(round => round.Stages).ToDictionary(stage => stage.Id, stage => stage.PrizeName);
		return Ok(new
		{
			eventInfo = new { bingoEvent.Name, bingoEvent.PublicCode, bingoEvent.Status, bingoEvent.CreatedAt },
			participants,
			cards,
			rounds = rounds.Select(round => new
			{
				round.Name,
				round.Sequence,
				round.Status,
				round.SequenceHash,
				fullSequence = round.Status is RoundStatus.Finished or RoundStatus.Cancelled ? round.DrawSequence : null,
				drawnNumbers = round.DrawnNumbers.OrderBy(item => item.Sequence).Select(item => new { item.Number, item.Sequence, item.DrawnAt }),
				stages = round.Stages.OrderBy(item => item.Sequence).Select(item => new { item.PrizeName, item.Pattern, item.IsCompleted }),
				winners = winners.Where(winner => winner.RoundId == round.Id).Select(winner => new { participantName = participantNames.GetValueOrDefault(winner.ParticipantId, "Participante indisponível"), prizeName = stageNames.GetValueOrDefault(winner.StageId, "Prêmio"), winner.IsWinner, winner.TieBreakerNumber, winner.DetectedAt, winner.RevealedAt })
			}),
			entries = await db.AuditEntries.Where(item => item.EventId == bingoEvent.Id).OrderBy(item => item.OccurredAt).Select(item => new { item.Action, item.Details, item.OccurredAt }).ToListAsync()
		});
	}

	private static BingoRound? SelectRound(BingoEvent bingoEvent) => bingoEvent.Rounds
		.Where(item => item.Status is RoundStatus.Drawing or RoundStatus.WinnerDetected or RoundStatus.TieBreaker)
		.OrderByDescending(item => item.Sequence)
		.FirstOrDefault()
		?? bingoEvent.Rounds.Where(item => item.Status == RoundStatus.Ready).OrderBy(item => item.Sequence).FirstOrDefault()
		?? bingoEvent.Rounds.OrderByDescending(item => item.Sequence).FirstOrDefault();

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
