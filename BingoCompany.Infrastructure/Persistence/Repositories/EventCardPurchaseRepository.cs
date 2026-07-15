using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class EventCardPurchaseRepository(BingoDbContext db) : IEventCardPurchaseRepository
{
	public Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken) => db.Events.SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);

	public Task<int> CountActivePurchasedCards(Guid eventId, CancellationToken cancellationToken) => db.Cards
		.Join(db.Participants, card => card.ParticipantId, participant => participant.Id, (card, participant) => new { card, participant })
		.CountAsync(item => item.card.EventId == eventId && item.card.Type == CardType.Digital && item.participant.ParticipantAccountId.HasValue && item.card.Status != CardStatus.Cancelled, cancellationToken);

	public async Task<IReadOnlyCollection<BingoCard>> GetPurchasedCards(Guid eventId, CancellationToken cancellationToken) => await db.Cards
		.Join(db.Participants, card => card.ParticipantId, participant => participant.Id, (card, participant) => new { card, participant })
		.Where(item => item.card.EventId == eventId && item.card.Type == CardType.Digital && item.participant.ParticipantAccountId.HasValue && item.card.Status != CardStatus.Cancelled)
		.Select(item => item.card)
		.ToListAsync(cancellationToken);

	public void AddAuditEntry(AuditEntry entry) => db.AuditEntries.Add(entry);

	public Task SaveChanges(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
