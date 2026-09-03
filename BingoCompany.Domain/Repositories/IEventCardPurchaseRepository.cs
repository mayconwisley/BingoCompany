using BingoCompany.Domain.Entities;

namespace BingoCompany.Domain.Repositories;

public interface IEventCardPurchaseRepository
{
	Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken);
	Task<int> CountActivePurchasedCards(Guid eventId, CancellationToken cancellationToken);
	Task<IReadOnlyCollection<BingoCard>> GetPurchasedCards(Guid eventId, CancellationToken cancellationToken);
	void AddInvitation(CardPurchaseInvitation invitation);
	void AddAuditEntry(AuditEntry entry);
	Task SaveChanges(CancellationToken cancellationToken);
}
