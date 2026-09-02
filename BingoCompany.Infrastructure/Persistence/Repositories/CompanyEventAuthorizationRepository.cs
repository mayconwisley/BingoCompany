using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class CompanyEventAuthorizationRepository(BingoDbContext db) : ICompanyEventAuthorizationRepository
{
	public Task<bool> OwnsEvent(Guid eventId, Guid companyId, CancellationToken cancellationToken)
	{
		return db.Events.AnyAsync(item => item.Id == eventId && item.CompanyId == companyId, cancellationToken);
	}
}
