using BingoCompany.Api.Contracts;
using BingoCompany.Api.Interfaces;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Api.Controllers;

public sealed partial class PublicController
{
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

}
