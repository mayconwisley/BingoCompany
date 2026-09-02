using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Queries;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class PublicEventAuditReadRepository(BingoDbContext db) : IPublicEventAuditReadRepository
{
	public async Task<PublicEventAuditResult?> Get(string publicCode, int page, int pageSize, CancellationToken cancellationToken)
	{
		var bingoEvent = await db.Events.AsNoTracking().SingleOrDefaultAsync(item => item.PublicCode == publicCode, cancellationToken);
		if (bingoEvent is null) return null;
		var rounds = await db.Rounds.AsNoTracking().Where(item => item.EventId == bingoEvent.Id).OrderBy(item => item.Sequence).Include(item => item.Stages).Include(item => item.DrawnNumbers).ToListAsync(cancellationToken);
		var participants = await db.Participants.AsNoTracking().Where(item => item.EventId == bingoEvent.Id).OrderBy(item => item.JoinedAt).Select(item => new PublicAuditParticipant(item.Id, item.Name, item.JoinedAt)).ToListAsync(cancellationToken);
		var cards = await db.Cards.AsNoTracking().Where(item => item.EventId == bingoEvent.Id).OrderBy(item => item.CreatedAt).Select(item => new PublicAuditCard(item.Id, item.PublicCode, item.Type, item.Status, item.ParticipantId, item.CreatedAt)).ToListAsync(cancellationToken);
		var winners = await db.RoundWinners.AsNoTracking().Where(item => rounds.Select(round => round.Id).Contains(item.RoundId)).ToListAsync(cancellationToken);
		var auditEntries = db.AuditEntries.AsNoTracking().Where(item => item.EventId == bingoEvent.Id);
		var totalEntries = await auditEntries.CountAsync(cancellationToken);
		var entries = await auditEntries
			.OrderByDescending(item => item.OccurredAt)
			.ThenByDescending(item => item.Id)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(item => new PublicAuditEntry(item.Action, item.Details, item.OccurredAt))
			.ToListAsync(cancellationToken);
		var participantNames = participants.ToDictionary(item => item.Id, item => item.Name);
		var cardCodes = cards.ToDictionary(item => item.Id, item => item.PublicCode);
		var stageNames = rounds.SelectMany(round => round.Stages).ToDictionary(stage => stage.Id, stage => stage.PrizeName);
		return new PublicEventAuditResult(
			new PublicAuditEventInfo(bingoEvent.Name, bingoEvent.PublicCode, bingoEvent.Status, bingoEvent.CreatedAt),
			participants,
			cards,
			rounds.Select(round => new PublicAuditRound(round.Name, round.Sequence, round.Status, round.SequenceHash, round.Status is RoundStatus.Finished or RoundStatus.Cancelled ? round.DrawSequence : null, round.DrawnNumbers.OrderBy(item => item.Sequence).Select(item => new PublicAuditDrawnNumber(item.Number, item.Sequence, item.DrawnAt)).ToArray(), round.Stages.OrderBy(item => item.Sequence).Select(item => new PublicAuditStage(item.PrizeName, item.Pattern, item.IsCompleted)).ToArray(), winners.Where(winner => winner.RoundId == round.Id).Select(winner => new PublicAuditWinner(participantNames.GetValueOrDefault(winner.ParticipantId, "Participante indisponível"), cardCodes.GetValueOrDefault(winner.CardId, "Cartela indisponível"), stageNames.GetValueOrDefault(winner.StageId, "Prêmio"), winner.IsWinner, winner.TieBreakerNumber, winner.DetectedAt, winner.RevealedAt)).ToArray())).ToArray(),
			new PublicAuditEntriesPage(entries, page, pageSize, totalEntries, (int)Math.Ceiling(totalEntries / (double)pageSize)));
	}
}
