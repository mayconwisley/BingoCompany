using BingoCompany.Domain.Queries;

namespace BingoCompany.Domain.Repositories;

public interface ICompanyEventsReadRepository
{
	Task<CompanyEventsPage> List(Guid companyId, int page, int pageSize, CancellationToken cancellationToken);
	Task<CompanyEventDetails?> Get(Guid eventId, int awardedCardsPage, int awardedCardsPageSize, CancellationToken cancellationToken);
}
