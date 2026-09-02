using BingoCompany.Domain.Queries;

namespace BingoCompany.Domain.Repositories;

public interface IPublicEventReadRepository
{
	Task<PublicEventQueryResult?> Get(string publicCode, CancellationToken cancellationToken);
}
