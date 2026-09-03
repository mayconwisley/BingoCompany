using BingoCompany.Domain.Queries;

namespace BingoCompany.Application.Interfaces;

public interface ICompanyEventsQueryService
{
	Task<CompanyEventsPage> List(Guid companyId, int page, int pageSize, CancellationToken cancellationToken);
	Task<CompanyEventDetails?> Get(Guid eventId, int awardedCardsPage, int awardedCardsPageSize, CancellationToken cancellationToken);
}
