using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class RoundGameplayRepository(BingoDbContext db) : IRoundGameplayRepository
{
	public Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken) => db.Events.SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
	public Task<BingoRound?> GetRound(Guid eventId, Guid roundId, CancellationToken cancellationToken) => db.Rounds.Include(item => item.Stages).Include(item => item.DrawnNumbers).SingleOrDefaultAsync(item => item.Id == roundId && item.EventId == eventId, cancellationToken);
	public async Task<IReadOnlyCollection<BingoCard>> GetEligibleCards(Guid roundId, CancellationToken cancellationToken) => await db.Cards.AsNoTracking().Join(db.RoundEligibleCards.Where(item => item.RoundId == roundId), card => card.Id, eligibleCard => eligibleCard.CardId, (card, _) => card).ToListAsync(cancellationToken);
	public async Task<IReadOnlyCollection<CardMark>> GetMarks(Guid roundId, CancellationToken cancellationToken) => await db.CardMarks.Where(item => item.RoundId == roundId).ToListAsync(cancellationToken);
	public async Task<IReadOnlySet<Guid>> GetExcludedCardIds(Guid roundId, Guid stageId, CancellationToken cancellationToken) => await db.RoundWinners.Where(item => item.RoundId == roundId && item.StageId == stageId).Select(item => item.CardId).ToHashSetAsync(cancellationToken);
	public async Task<IReadOnlyCollection<RoundWinner>> GetCandidates(Guid roundId, Guid stageId, CancellationToken cancellationToken) => await db.RoundWinners.Where(item => item.RoundId == roundId && item.StageId == stageId && !item.PrizeDeliveredAt.HasValue && !item.PrizeDeclinedAt.HasValue).ToListAsync(cancellationToken);
	public async Task<RoundWinner?> GetPendingWinner(Guid roundId, Guid stageId, CancellationToken cancellationToken) => (await db.RoundWinners.Where(item => item.RoundId == roundId && item.StageId == stageId && item.IsWinner && item.RevealedAt.HasValue && !item.PrizeDeliveredAt.HasValue && !item.PrizeDeclinedAt.HasValue).ToListAsync(cancellationToken)).OrderByDescending(item => item.TieBreakerNumber).ThenBy(item => item.RevealedAt).ThenBy(item => item.Id).FirstOrDefault();
	public async Task<IReadOnlyDictionary<Guid, string>> GetParticipantNames(IReadOnlyCollection<Guid> participantIds, CancellationToken cancellationToken) => await db.Participants.Where(item => participantIds.Contains(item.Id)).ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
	public Task<BingoCard?> GetCard(Guid cardId, CancellationToken cancellationToken) => db.Cards.FindAsync([cardId], cancellationToken).AsTask();
	public void AddDrawnNumber(DrawnNumber drawnNumber) => db.DrawnNumbers.Add(drawnNumber);
	public void AddWinners(IReadOnlyCollection<RoundWinner> winners) => db.RoundWinners.AddRange(winners);
	public void AddAuditEntry(AuditEntry entry) => db.AuditEntries.Add(entry);
	public Task SaveChanges(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
	public async Task<bool> TrySaveChanges(CancellationToken cancellationToken) { try { await db.SaveChangesAsync(cancellationToken); return true; } catch (DbUpdateConcurrencyException) { return false; } }
}
