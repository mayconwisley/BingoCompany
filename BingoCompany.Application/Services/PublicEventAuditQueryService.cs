using BingoCompany.Application.Interfaces;
using BingoCompany.Domain.Queries;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class PublicEventAuditQueryService(IPublicEventAuditReadRepository repository) : IPublicEventAuditQueryService
{
	private const int MaximumPageSize = 100;

	public Task<PublicEventAuditResult?> Get(string publicCode, int page, int pageSize, CancellationToken cancellationToken)
	{
		var normalizedPage = Math.Max(page, 1);
		var normalizedPageSize = Math.Clamp(pageSize, 1, MaximumPageSize);
		return repository.Get(publicCode, normalizedPage, normalizedPageSize, cancellationToken);
	}
}
