using BingoCompany.Domain.Entities;

namespace BingoCompany.Domain.Repositories;

public interface IEventRoundManagementRepository
{
	Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken);
	Task<BingoEvent?> GetEventWithRounds(Guid eventId, CancellationToken cancellationToken);
	Task<BingoRound?> GetRoundWithStages(Guid eventId, Guid roundId, CancellationToken cancellationToken);
	Task<BingoRound?> GetRoundWithDrawnNumbers(Guid eventId, Guid roundId, CancellationToken cancellationToken);
	Task<int> CountRounds(Guid eventId, CancellationToken cancellationToken);
	Task<bool> HasActiveEligibleCard(Guid eventId, CancellationToken cancellationToken);
	Task<IReadOnlyCollection<BingoCard>> GetEventCards(Guid eventId, CancellationToken cancellationToken);
	Task<IReadOnlyCollection<BingoCard>> GetAssignedDigitalCards(Guid eventId, CancellationToken cancellationToken);
	void AddRound(BingoRound round);
	void AddPrizeStages(IReadOnlyCollection<PrizeStage> stages);
	void AddEligibleCards(IReadOnlyCollection<RoundEligibleCard> eligibleCards);
	void AddAuditEntry(AuditEntry entry);
	Task SaveChanges(CancellationToken cancellationToken);
	Task<bool> TrySaveChanges(CancellationToken cancellationToken);
}
