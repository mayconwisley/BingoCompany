using BingoCompany.Application.Interfaces;
using BingoCompany.Domain.Queries;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class PublicEventAuditQueryService(IPublicEventAuditReadRepository repository) : IPublicEventAuditQueryService
{
	public Task<PublicEventAuditResult?> Get(string publicCode, CancellationToken cancellationToken) => repository.Get(publicCode, cancellationToken);
}
