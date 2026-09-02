using BingoCompany.Domain.Entities;

namespace BingoCompany.Domain.Repositories;

public interface IEventParticipantRegistrationRepository
{
	Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken);
	Task<BingoEvent?> GetEventByPublicCode(string publicCode, CancellationToken cancellationToken);
	Task<ParticipantAccount?> GetParticipantAccount(Guid accountId, CancellationToken cancellationToken);
	Task<int> CountActivePurchasedCards(Guid eventId, CancellationToken cancellationToken);
	void AddParticipant(Participant participant);
	void AddCards(IReadOnlyCollection<BingoCard> cards);
	void AddAuditEntry(AuditEntry entry);
	Task SaveChanges(CancellationToken cancellationToken);
}
