using BingoCompany.Application.Interfaces;
using BingoCompany.Domain.Queries;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class CompanyEventsQueryService(ICompanyEventsReadRepository repository) : ICompanyEventsQueryService
{
	public Task<CompanyEventsPage> List(Guid companyId, int page, int pageSize, CancellationToken cancellationToken) => repository.List(companyId, page, pageSize, cancellationToken);

	public Task<CompanyEventDetails?> Get(Guid eventId, int awardedCardsPage, int awardedCardsPageSize, CancellationToken cancellationToken) => repository.Get(eventId, awardedCardsPage, awardedCardsPageSize, cancellationToken);
}
