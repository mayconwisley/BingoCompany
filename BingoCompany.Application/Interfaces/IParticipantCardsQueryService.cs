using BingoCompany.Application.Services;

namespace BingoCompany.Application.Interfaces;

public interface IParticipantCardsQueryService
{
	Task<ParticipantCardsPage> GetCards(Guid participantAccountId, int page, int pageSize, CancellationToken cancellationToken);
}
