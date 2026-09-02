using BingoCompany.Domain.Queries;

namespace BingoCompany.Domain.Repositories;

public interface IPublicEventAuditReadRepository
{
	Task<PublicEventAuditResult?> Get(string publicCode, CancellationToken cancellationToken);
}
