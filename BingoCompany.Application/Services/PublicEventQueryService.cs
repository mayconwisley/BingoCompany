using BingoCompany.Application.Interfaces;
using BingoCompany.Domain.Queries;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class PublicEventQueryService(IPublicEventReadRepository repository) : IPublicEventQueryService
{
	public Task<PublicEventQueryResult?> Get(string publicCode, CancellationToken cancellationToken)
	{
		return repository.Get(publicCode, cancellationToken);
	}
}
