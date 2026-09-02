using BingoCompany.Application.Interfaces;
using BingoCompany.Domain.Queries;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class PublicEventAuditQueryService(IPublicEventAuditReadRepository repository) : IPublicEventAuditQueryService
{
	private const int MaximumPageSize = 100;

	public Task<PublicEventAuditResult?> Get(string publicCode, int page, int pageSize, int? roundSequence, CancellationToken cancellationToken)
	{
		var normalizedPage = Math.Max(page, 1);
		var normalizedPageSize = Math.Clamp(pageSize, 1, MaximumPageSize);
		var normalizedRoundSequence = roundSequence is > 0 ? roundSequence : null;
		return repository.Get(publicCode, normalizedPage, normalizedPageSize, normalizedRoundSequence, cancellationToken);
	}
}
