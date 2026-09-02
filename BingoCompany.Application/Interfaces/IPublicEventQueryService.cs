using BingoCompany.Domain.Queries;

namespace BingoCompany.Application.Interfaces;

public interface IPublicEventQueryService
{
	Task<PublicEventQueryResult?> Get(string publicCode, CancellationToken cancellationToken);
}
