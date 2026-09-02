using BingoCompany.Application.Interfaces;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class CompanyEventAuthorizationService(ICompanyEventAuthorizationRepository repository) : ICompanyEventAuthorizationService
{
	public Task<bool> OwnsEvent(Guid eventId, Guid companyId, CancellationToken cancellationToken)
	{
		return repository.OwnsEvent(eventId, companyId, cancellationToken);
	}
}
