namespace BingoCompany.Application.Interfaces;

public interface ICompanyEventAuthorizationService
{
	Task<bool> OwnsEvent(Guid eventId, Guid companyId, CancellationToken cancellationToken);
}
