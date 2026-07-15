using BingoCompany.Domain.Entities;

namespace BingoCompany.Domain.Repositories;

public interface IPrintedWinnerValidationRepository
{
	Task<BingoRound?> GetRound(Guid eventId, Guid roundId, CancellationToken cancellationToken);
	Task<BingoCard?> GetCard(Guid eventId, string cardCode, CancellationToken cancellationToken);
	Task<bool> IsEligible(Guid roundId, Guid cardId, CancellationToken cancellationToken);
	Task<bool> HasCandidate(Guid roundId, Guid stageId, Guid cardId, CancellationToken cancellationToken);
	Task<int> CountCandidates(Guid roundId, Guid stageId, CancellationToken cancellationToken);
	Task<string> GetParticipantName(Guid participantId, CancellationToken cancellationToken);
	void AddWinner(RoundWinner winner);
	void AddAuditEntry(AuditEntry entry);
	Task SaveChanges(CancellationToken cancellationToken);
}
