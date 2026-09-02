using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class EventParticipantRegistrationRepository(BingoDbContext db) : IEventParticipantRegistrationRepository
{
	public Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken)
	{
		return db.Events.SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
	}

	public Task<BingoEvent?> GetEventByPublicCode(string publicCode, CancellationToken cancellationToken)
	{
		return db.Events.SingleOrDefaultAsync(item => item.PublicCode == publicCode, cancellationToken);
	}

	public Task<ParticipantAccount?> GetParticipantAccount(Guid accountId, CancellationToken cancellationToken)
	{
		return db.ParticipantAccounts.SingleOrDefaultAsync(item => item.Id == accountId, cancellationToken);
	}

	public Task<int> CountActivePurchasedCards(Guid eventId, CancellationToken cancellationToken)
	{
		return db.Cards
			.Join(db.Participants, card => card.ParticipantId, participant => participant.Id, (card, participant) => new { card, participant })
			.CountAsync(item => item.card.EventId == eventId
				&& item.card.Type == CardType.Digital
				&& item.participant.ParticipantAccountId.HasValue
				&& item.card.Status != CardStatus.Cancelled, cancellationToken);
	}

	public void AddParticipant(Participant participant) => db.Participants.Add(participant);

	public void AddCards(IReadOnlyCollection<BingoCard> cards) => db.Cards.AddRange(cards);

	public void AddAuditEntry(AuditEntry entry) => db.AuditEntries.Add(entry);

	public Task SaveChanges(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
