using BingoCompany.Domain.Queries;

namespace BingoCompany.Application.Interfaces;

public interface ICardStateQueryService
{
	Task<CardStateSnapshot?> Get(Guid eventId, string cardCode, CancellationToken cancellationToken);
}
