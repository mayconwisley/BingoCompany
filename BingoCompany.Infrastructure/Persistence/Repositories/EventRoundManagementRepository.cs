using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class EventRoundManagementRepository(BingoDbContext db) : IEventRoundManagementRepository
{
	public Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken) => db.Events.SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
	public Task<BingoEvent?> GetEventWithRounds(Guid eventId, CancellationToken cancellationToken) => db.Events.Include(item => item.Rounds).SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
	public Task<BingoRound?> GetRoundWithStages(Guid eventId, Guid roundId, CancellationToken cancellationToken) => db.Rounds.Include(item => item.Stages).SingleOrDefaultAsync(item => item.Id == roundId && item.EventId == eventId, cancellationToken);
	public Task<BingoRound?> GetRoundWithDrawnNumbers(Guid eventId, Guid roundId, CancellationToken cancellationToken) => db.Rounds.Include(item => item.DrawnNumbers).SingleOrDefaultAsync(item => item.Id == roundId && item.EventId == eventId, cancellationToken);
	public Task<int> CountRounds(Guid eventId, CancellationToken cancellationToken) => db.Rounds.CountAsync(item => item.EventId == eventId, cancellationToken);
	public Task<bool> HasActiveEligibleCard(Guid eventId, CancellationToken cancellationToken) => db.Cards.AnyAsync(item => item.EventId == eventId && item.Status == CardStatus.Active && item.ParticipantId.HasValue, cancellationToken);
	public async Task<IReadOnlyCollection<BingoCard>> GetEventCards(Guid eventId, CancellationToken cancellationToken) => await db.Cards.Where(item => item.EventId == eventId).ToListAsync(cancellationToken);
	public async Task<IReadOnlyCollection<BingoCard>> GetAssignedDigitalCards(Guid eventId, CancellationToken cancellationToken) => await db.Cards.Where(item => item.EventId == eventId && item.Type == CardType.Digital && item.Status == CardStatus.Assigned).ToListAsync(cancellationToken);
	public void AddRound(BingoRound round) => db.Rounds.Add(round);
	public void AddPrizeStages(IReadOnlyCollection<PrizeStage> stages) => db.PrizeStages.AddRange(stages);
	public void AddEligibleCards(IReadOnlyCollection<RoundEligibleCard> eligibleCards) => db.RoundEligibleCards.AddRange(eligibleCards);
	public void AddAuditEntry(AuditEntry entry) => db.AuditEntries.Add(entry);
	public Task SaveChanges(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
	public async Task<bool> TrySaveChanges(CancellationToken cancellationToken)
	{
		try
		{
			await db.SaveChangesAsync(cancellationToken);
			return true;
		}
		catch (DbUpdateConcurrencyException)
		{
			return false;
		}
	}
}
