using BingoCompany.Domain.Entities;

namespace BingoCompany.Domain.Repositories;

public interface IPrintedCardRegistrationRepository
{
	Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken);
	Task<BingoCard?> GetCard(Guid eventId, string cardCode, CancellationToken cancellationToken);
	void AddParticipant(Participant participant);
	void AddAuditEntry(AuditEntry entry);
	Task SaveChanges(CancellationToken cancellationToken);
}
