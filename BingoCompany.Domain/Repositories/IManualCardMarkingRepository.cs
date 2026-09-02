using BingoCompany.Domain.Entities;

namespace BingoCompany.Domain.Repositories;

public interface IManualCardMarkingRepository
{
	Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken);
	Task<BingoCard?> GetCard(Guid eventId, string cardCode, CancellationToken cancellationToken);
	Task<BingoRound?> GetDrawingRound(Guid eventId, CancellationToken cancellationToken);
	Task<bool> HasMark(Guid roundId, Guid cardId, int number, CancellationToken cancellationToken);
	Task<IReadOnlyCollection<int>> GetMarkedNumbers(Guid roundId, Guid cardId, CancellationToken cancellationToken);
	Task<bool> IsExcludedFromActiveStage(Guid roundId, Guid stageId, Guid cardId, CancellationToken cancellationToken);
	void AddMark(CardMark mark);
	void AddWinner(RoundWinner winner);
	void AddAuditEntry(AuditEntry entry);
	Task SaveChanges(CancellationToken cancellationToken);
}
