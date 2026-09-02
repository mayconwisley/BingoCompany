using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class EventConfigurationRepository(BingoDbContext db) : IEventConfigurationRepository
{
	public Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken)
	{
		return db.Events.SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
	}

	public void AddEvent(BingoEvent bingoEvent) => db.Events.Add(bingoEvent);

	public void AddAuditEntry(AuditEntry entry) => db.AuditEntries.Add(entry);

	public Task SaveChanges(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
