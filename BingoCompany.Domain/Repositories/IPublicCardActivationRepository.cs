using BingoCompany.Domain.Entities;

namespace BingoCompany.Domain.Repositories;

public interface IPublicCardActivationRepository
{
	Task<BingoEvent?> GetEvent(string publicCode, CancellationToken cancellationToken);
	Task<BingoCard?> GetCard(Guid eventId, string cardCode, CancellationToken cancellationToken);
	Task<Guid?> GetParticipantAccountId(Guid participantId, CancellationToken cancellationToken);
	void AddAuditEntry(AuditEntry entry);
	Task SaveChanges(CancellationToken cancellationToken);
}
