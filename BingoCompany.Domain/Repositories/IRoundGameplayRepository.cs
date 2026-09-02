using BingoCompany.Domain.Entities;

namespace BingoCompany.Domain.Repositories;

public interface IRoundGameplayRepository
{
	Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken);
	Task<BingoRound?> GetRound(Guid eventId, Guid roundId, CancellationToken cancellationToken);
	Task<IReadOnlyCollection<BingoCard>> GetEligibleCards(Guid roundId, CancellationToken cancellationToken);
	Task<IReadOnlyCollection<CardMark>> GetMarks(Guid roundId, CancellationToken cancellationToken);
	Task<IReadOnlySet<Guid>> GetExcludedCardIds(Guid roundId, Guid stageId, CancellationToken cancellationToken);
	Task<IReadOnlyCollection<RoundWinner>> GetCandidates(Guid roundId, Guid stageId, CancellationToken cancellationToken);
	Task<RoundWinner?> GetPendingWinner(Guid roundId, Guid stageId, CancellationToken cancellationToken);
	Task<IReadOnlyDictionary<Guid, string>> GetParticipantNames(IReadOnlyCollection<Guid> participantIds, CancellationToken cancellationToken);
	Task<BingoCard?> GetCard(Guid cardId, CancellationToken cancellationToken);
	void AddDrawnNumber(DrawnNumber drawnNumber);
	void AddWinners(IReadOnlyCollection<RoundWinner> winners);
	void AddAuditEntry(AuditEntry entry);
	Task SaveChanges(CancellationToken cancellationToken);
	Task<bool> TrySaveChanges(CancellationToken cancellationToken);
}
