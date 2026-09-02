using BingoCompany.Domain.Queries;

namespace BingoCompany.Domain.Repositories;

public interface ICardStateReadRepository
{
	Task<CardStateSnapshot?> Get(Guid eventId, string cardCode, CancellationToken cancellationToken);
}
