namespace BingoCompany.Domain.Repositories;

public interface ICompanyEventAuthorizationRepository
{
	Task<bool> OwnsEvent(Guid eventId, Guid companyId, CancellationToken cancellationToken);
}
