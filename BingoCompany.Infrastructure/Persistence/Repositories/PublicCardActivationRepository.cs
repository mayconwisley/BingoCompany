using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class PublicCardActivationRepository(BingoDbContext db) : IPublicCardActivationRepository
{
	public Task<BingoEvent?> GetEvent(string publicCode, CancellationToken cancellationToken)
	{
		return db.Events.SingleOrDefaultAsync(item => item.PublicCode == publicCode, cancellationToken);
	}

	public Task<BingoCard?> GetCard(Guid eventId, string cardCode, CancellationToken cancellationToken)
	{
		return db.Cards.SingleOrDefaultAsync(item => item.EventId == eventId && item.PublicCode == cardCode, cancellationToken);
	}

	public Task<Guid?> GetParticipantAccountId(Guid participantId, CancellationToken cancellationToken)
	{
		return db.Participants.Where(item => item.Id == participantId).Select(item => item.ParticipantAccountId).SingleOrDefaultAsync(cancellationToken);
	}

	public void AddAuditEntry(AuditEntry entry) => db.AuditEntries.Add(entry);

	public Task SaveChanges(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
