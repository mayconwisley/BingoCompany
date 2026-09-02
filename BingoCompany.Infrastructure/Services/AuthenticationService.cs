using BingoCompany.Application.Interfaces;
using BingoCompany.Domain.Entities;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Services;

public sealed class AuthenticationService(BingoDbContext db) : IAuthenticationService
{
	private readonly PasswordHasher<CompanyUser> _companyPasswordHasher = new();
	private readonly PasswordHasher<ParticipantAccount> _participantPasswordHasher = new();

	public async Task<CompanyAuthenticationResult> RegisterCompany(string companyName, string userName, string email, string password, CancellationToken cancellationToken)
	{
		var normalizedEmail = email.Trim().ToLowerInvariant();
		if (await db.CompanyUsers.AnyAsync(item => item.Email == normalizedEmail, cancellationToken)) throw new InvalidOperationException("Já existe uma conta cadastrada com este e-mail. Entre para acessar sua conta ou use outro e-mail.");
		var company = new Company(companyName.Trim());
		var unhashedUser = new CompanyUser(company.Id, userName.Trim(), normalizedEmail, string.Empty);
		var user = new CompanyUser(company.Id, userName.Trim(), normalizedEmail, _companyPasswordHasher.HashPassword(unhashedUser, password));
		db.Companies.Add(company);
		db.CompanyUsers.Add(user);
		await db.SaveChangesAsync(cancellationToken);
		return new CompanyAuthenticationResult(user.Id, company.Id, user.Name, user.Email, company.Name);
	}

	public async Task<CompanyAuthenticationResult?> LoginCompany(string email, string password, CancellationToken cancellationToken)
	{
		var normalizedEmail = email.Trim().ToLowerInvariant();
		var user = await db.CompanyUsers.SingleOrDefaultAsync(item => item.Email == normalizedEmail, cancellationToken);
		if (user is null) return null;
		var verification = _companyPasswordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
		if (verification == PasswordVerificationResult.Failed) return null;
		if (verification == PasswordVerificationResult.SuccessRehashNeeded)
		{
			user.UpdatePasswordHash(_companyPasswordHasher.HashPassword(user, password));
			await db.SaveChangesAsync(cancellationToken);
		}
		var companyName = await db.Companies.Where(item => item.Id == user.CompanyId).Select(item => item.Name).SingleAsync(cancellationToken);
		return new CompanyAuthenticationResult(user.Id, user.CompanyId, user.Name, user.Email, companyName);
	}

	public Task<string?> GetCompanyName(Guid companyId, CancellationToken cancellationToken) => db.Companies.Where(item => item.Id == companyId).Select(item => item.Name).SingleOrDefaultAsync(cancellationToken);

	public async Task<ParticipantAuthenticationResult> RegisterParticipant(string name, string email, string password, CancellationToken cancellationToken)
	{
		var normalizedEmail = email.Trim().ToLowerInvariant();
		if (await db.ParticipantAccounts.AnyAsync(item => item.Email == normalizedEmail, cancellationToken)) throw new InvalidOperationException("Já existe uma conta de participante com este e-mail.");
		var unhashedAccount = new ParticipantAccount(name, normalizedEmail, string.Empty);
		var account = new ParticipantAccount(name, normalizedEmail, _participantPasswordHasher.HashPassword(unhashedAccount, password));
		db.ParticipantAccounts.Add(account);
		await db.SaveChangesAsync(cancellationToken);
		return new ParticipantAuthenticationResult(account.Id, account.Name, account.Email);
	}

	public async Task<ParticipantAuthenticationResult?> LoginParticipant(string email, string password, CancellationToken cancellationToken)
	{
		var normalizedEmail = email.Trim().ToLowerInvariant();
		var account = await db.ParticipantAccounts.SingleOrDefaultAsync(item => item.Email == normalizedEmail, cancellationToken);
		if (account is null || _participantPasswordHasher.VerifyHashedPassword(account, account.PasswordHash, password) == PasswordVerificationResult.Failed) return null;
		return new ParticipantAuthenticationResult(account.Id, account.Name, account.Email);
	}
}
