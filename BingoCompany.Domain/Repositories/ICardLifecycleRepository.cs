using BingoCompany.Domain.Entities;

namespace BingoCompany.Domain.Repositories;

public interface ICardLifecycleRepository
{
	Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken);
	Task<BingoCard?> GetCard(Guid eventId, string cardCode, CancellationToken cancellationToken);
	Task<BingoRound?> GetLatestCompletedRound(Guid eventId, CancellationToken cancellationToken);
	Task<bool> HasStartedRoundAfter(Guid eventId, int sequence, CancellationToken cancellationToken);
	Task<bool> WasEligible(Guid roundId, Guid cardId, CancellationToken cancellationToken);
	Task<string?> GetCompanyName(Guid eventId, CancellationToken cancellationToken);
	Task<IReadOnlyCollection<BingoCard>> GetPrintedCards(Guid eventId, CancellationToken cancellationToken);
	Task<Participant?> GetParticipant(Guid eventId, Guid participantId, CancellationToken cancellationToken);
	void AddCard(BingoCard card);
	void AddCards(IReadOnlyCollection<BingoCard> cards);
	void AddAuditEntry(AuditEntry entry);
	Task SaveChanges(CancellationToken cancellationToken);
}
