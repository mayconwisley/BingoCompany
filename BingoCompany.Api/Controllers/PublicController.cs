using BingoCompany.Domain;
using BingoCompany.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/public/events")]
public sealed class PublicController(BingoDbContext db) : ControllerBase
{
    [HttpGet("{code}")]
    public async Task<ActionResult<object>> Get(string code)
    {
        var e = await db.Events.Include(x => x.Rounds).ThenInclude(x => x.Stages).Include(x => x.Rounds).ThenInclude(x => x.DrawnNumbers).SingleOrDefaultAsync(x => x.PublicCode == code); if (e is null) return NotFound();
        var round = e.Rounds.OrderByDescending(x => x.Sequence).FirstOrDefault(x => x.Status != RoundStatus.Ready) ?? e.Rounds.OrderBy(x => x.Sequence).FirstOrDefault();
        var winner = round is null ? null : (await db.RoundWinners.Where(x => x.RoundId == round.Id && x.IsWinner).ToListAsync()).OrderByDescending(x => x.RevealedAt).FirstOrDefault(); var winnerName = winner is null ? null : (await db.Participants.FindAsync(winner.ParticipantId))?.Name;
        return Ok(new { e.Id, e.Name, e.PublicCode, e.Status, e.MarkingMode, participants = await db.Participants.CountAsync(x => x.EventId == e.Id), cards = await db.Cards.CountAsync(x => x.EventId == e.Id), round = round is null ? null : new { round.Id, round.Name, round.Sequence, round.Status, round.SequenceHash, currentPrize = round.Stages.SingleOrDefault(x => x.IsActive)?.PrizeName, stages = round.Stages.OrderBy(x => x.Sequence).Select(x => new { x.PrizeName, x.Pattern, x.IsActive, x.IsCompleted }), drawnNumbers = round.DrawnNumbers.OrderBy(x => x.Sequence).Select(x => x.Number), winnerName } });
    }
    [HttpPost("{code}/join")]
    public async Task<ActionResult<object>> Join(string code, JoinEventRequest request) { var e = await db.Events.SingleOrDefaultAsync(x => x.PublicCode == code); if (e is null) return NotFound(); return await new EventsController(db, HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<BingoCompany.Api.Hubs.BingoHub>>()).Join(e.Id, request); }
}
