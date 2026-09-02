using BingoCompany.Application.Interfaces;
using BingoCompany.Domain.Queries;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class CardStateQueryService(ICardStateReadRepository repository) : ICardStateQueryService
{
	public Task<CardStateSnapshot?> Get(Guid eventId, string cardCode, CancellationToken cancellationToken) => repository.Get(eventId, cardCode, cancellationToken);
}
