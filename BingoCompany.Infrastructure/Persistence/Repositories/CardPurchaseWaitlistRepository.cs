using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class CardPurchaseWaitlistRepository(BingoDbContext db) : ICardPurchaseWaitlistRepository
{
	public Task<BingoEvent?> GetEventByPublicCode(string publicCode, CancellationToken cancellationToken) => db.Events.SingleOrDefaultAsync(item => item.PublicCode == publicCode, cancellationToken);

	public Task<int> CountActivePurchasedCards(Guid eventId, CancellationToken cancellationToken) => db
		.PurchasedDigitalCards(eventId)
		.CountAsync(cancellationToken);

	public Task<CardPurchaseWaitlistEntry?> GetEntry(Guid eventId, Guid participantAccountId, CancellationToken cancellationToken) => db.CardPurchaseWaitlistEntries
		.SingleOrDefaultAsync(item => item.EventId == eventId && item.ParticipantAccountId == participantAccountId, cancellationToken);

	public Task<int> CountEntriesBefore(Guid eventId, DateTimeOffset createdAt, CancellationToken cancellationToken) => db.CardPurchaseWaitlistEntries
		.CountAsync(item => item.EventId == eventId && item.CreatedAt < createdAt, cancellationToken);

	public void Add(CardPurchaseWaitlistEntry entry) => db.CardPurchaseWaitlistEntries.Add(entry);
	public void AddAuditEntry(AuditEntry entry) => db.AuditEntries.Add(entry);
	public Task SaveChanges(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
