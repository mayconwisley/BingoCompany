using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class PrintedWinnerValidationRepository(BingoDbContext db) : IPrintedWinnerValidationRepository
{
	public Task<BingoRound?> GetRound(Guid eventId, Guid roundId, CancellationToken cancellationToken) => db.Rounds.Include(item => item.Stages).Include(item => item.DrawnNumbers).SingleOrDefaultAsync(item => item.Id == roundId && item.EventId == eventId, cancellationToken);
	public Task<BingoCard?> GetCard(Guid eventId, string cardCode, CancellationToken cancellationToken) => db.Cards.SingleOrDefaultAsync(item => item.EventId == eventId && item.PublicCode == cardCode, cancellationToken);
	public Task<bool> IsEligible(Guid roundId, Guid cardId, CancellationToken cancellationToken) => db.RoundEligibleCards.AnyAsync(item => item.RoundId == roundId && item.CardId == cardId, cancellationToken);
	public Task<bool> HasCandidate(Guid roundId, Guid stageId, Guid cardId, CancellationToken cancellationToken) => db.RoundWinners.AnyAsync(item => item.RoundId == roundId && item.StageId == stageId && item.CardId == cardId, cancellationToken);
	public Task<int> CountCandidates(Guid roundId, Guid stageId, CancellationToken cancellationToken) => db.RoundWinners.CountAsync(item => item.RoundId == roundId && item.StageId == stageId, cancellationToken);
	public Task<string> GetParticipantName(Guid participantId, CancellationToken cancellationToken) => db.Participants.Where(item => item.Id == participantId).Select(item => item.Name).SingleAsync(cancellationToken);
	public void AddWinner(RoundWinner winner) => db.RoundWinners.Add(winner);
	public void AddAuditEntry(AuditEntry entry) => db.AuditEntries.Add(entry);
	public Task SaveChanges(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
