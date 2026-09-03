using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class EventCardPurchaseRepository(BingoDbContext db) : IEventCardPurchaseRepository
{
	public Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken) => db.Events.SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);

	public Task<int> CountActivePurchasedCards(Guid eventId, CancellationToken cancellationToken) => db
		.PurchasedDigitalCards(eventId)
		.CountAsync(cancellationToken);

	public async Task<IReadOnlyCollection<BingoCard>> GetPurchasedCards(Guid eventId, CancellationToken cancellationToken) => await db
		.PurchasedDigitalCards(eventId)
		.ToListAsync(cancellationToken);

	public void AddAuditEntry(AuditEntry entry) => db.AuditEntries.Add(entry);

	public void AddInvitation(CardPurchaseInvitation invitation) => db.CardPurchaseInvitations.Add(invitation);

	public Task SaveChanges(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
