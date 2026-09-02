using BingoCompany.Domain.Entities;

namespace BingoCompany.Domain.Repositories;

public interface IEventConfigurationRepository
{
	Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken);
	void AddEvent(BingoEvent bingoEvent);
	void AddAuditEntry(AuditEntry entry);
	Task SaveChanges(CancellationToken cancellationToken);
}
