using BingoCompany.Application.Interfaces;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class ParticipantCardsQueryService(IParticipantCardsRepository repository) : IParticipantCardsQueryService
{
	public async Task<ParticipantCardsPage> GetCards(Guid participantAccountId, int page, int pageSize, CancellationToken cancellationToken)
	{
		var normalizedPage = Math.Max(1, page);
		var normalizedPageSize = Math.Clamp(pageSize, 1, 50);
		var activeCards = await repository.GetActiveCards(participantAccountId, cancellationToken);
		var totalItems = await repository.CountHistoryCards(participantAccountId, cancellationToken);
		var historyItems = await repository.GetHistoryCards(participantAccountId, (normalizedPage - 1) * normalizedPageSize, normalizedPageSize, cancellationToken);
		return new ParticipantCardsPage(activeCards, historyItems, normalizedPage, normalizedPageSize, totalItems, (int)Math.Ceiling(totalItems / (double)normalizedPageSize));
	}
}
