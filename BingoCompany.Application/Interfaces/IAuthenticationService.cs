namespace BingoCompany.Application.Interfaces;

public interface IAuthenticationService
{
	Task<CompanyAuthenticationResult> RegisterCompany(string companyName, string userName, string email, string password, CancellationToken cancellationToken);
	Task<CompanyAuthenticationResult?> LoginCompany(string email, string password, CancellationToken cancellationToken);
	Task<string?> GetCompanyName(Guid companyId, CancellationToken cancellationToken);
	Task<ParticipantAuthenticationResult> RegisterParticipant(string name, string email, string password, CancellationToken cancellationToken);
	Task<ParticipantAuthenticationResult?> LoginParticipant(string email, string password, CancellationToken cancellationToken);
}
