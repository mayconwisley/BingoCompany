using BingoCompany.Application;
using BingoCompany.Domain;
using BingoCompany.Infrastructure;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/public/events")]
public sealed class PublicController(BingoDbContext db) : ControllerBase
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
        var presentationStage = round?.Stages.OrderByDescending(item => item.Sequence).FirstOrDefault(item => item.IsCompleted && !item.IsWinnerPresentationClosed);
        var winner = presentationStage is null
            ? null
            : await db.RoundWinners.Where(item => item.RoundId == round!.Id && item.StageId == presentationStage.Id && item.IsWinner).SingleOrDefaultAsync();
        var winnerName = winner is null ? null : (await db.Participants.FindAsync(winner.ParticipantId))?.Name;
        var winnerDetectedCount = activeStage is null || round is null
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
                winner = winner is null ? null : new { participantName = winnerName, prizeName = presentationStage!.PrizeName, pattern = presentationStage.Pattern, prizeImageDataUrl = presentationStage.PrizeImageDataUrl }
            }
        });
    }

    [HttpPost("{code}/join")]
    public async Task<ActionResult<object>> Join(string code, JoinEventRequest request)
    {
        var bingoEvent = await db.Events.SingleOrDefaultAsync(item => item.PublicCode == code);
        if (bingoEvent is null) return NotFound();
        return await new EventsController(db, HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<BingoCompany.Api.Hubs.BingoHub>>()).Join(bingoEvent.Id, request);
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
                fullSequence = round.Status == RoundStatus.Finished ? round.DrawSequence : null,
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
