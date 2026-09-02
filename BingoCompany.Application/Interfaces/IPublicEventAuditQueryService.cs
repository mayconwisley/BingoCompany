using BingoCompany.Domain.Queries;

namespace BingoCompany.Application.Interfaces;

public interface IPublicEventAuditQueryService
{
	Task<PublicEventAuditResult?> Get(string publicCode, int page, int pageSize, CancellationToken cancellationToken);
}
