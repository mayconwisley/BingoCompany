using BingoCompany.Domain.Queries;

namespace BingoCompany.Domain.Repositories;

public interface IParticipantCardsRepository
{
	Task<IReadOnlyCollection<ParticipantCardSummary>> GetActiveCards(Guid participantAccountId, CancellationToken cancellationToken);
	Task<int> CountHistoryCards(Guid participantAccountId, CancellationToken cancellationToken);
	Task<IReadOnlyCollection<ParticipantCardSummary>> GetHistoryCards(Guid participantAccountId, int skip, int take, CancellationToken cancellationToken);
}
