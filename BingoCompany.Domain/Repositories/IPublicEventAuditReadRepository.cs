using BingoCompany.Domain.Queries;

namespace BingoCompany.Domain.Repositories;

public interface IPublicEventAuditReadRepository
{
	Task<PublicEventAuditResult?> Get(string publicCode, int page, int pageSize, int? roundSequence, CancellationToken cancellationToken);
}
