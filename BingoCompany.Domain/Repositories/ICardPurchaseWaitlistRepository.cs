using BingoCompany.Domain.Entities;

namespace BingoCompany.Domain.Repositories;

public interface ICardPurchaseWaitlistRepository
{
	Task<BingoEvent?> GetEventByPublicCode(string publicCode, CancellationToken cancellationToken);
	Task<int> CountActivePurchasedCards(Guid eventId, CancellationToken cancellationToken);
	Task<CardPurchaseWaitlistEntry?> GetEntry(Guid eventId, Guid participantAccountId, CancellationToken cancellationToken);
	Task<int> CountEntriesBefore(Guid eventId, DateTimeOffset createdAt, CancellationToken cancellationToken);
	void Add(CardPurchaseWaitlistEntry entry);
	void AddAuditEntry(AuditEntry entry);
	Task SaveChanges(CancellationToken cancellationToken);
}
