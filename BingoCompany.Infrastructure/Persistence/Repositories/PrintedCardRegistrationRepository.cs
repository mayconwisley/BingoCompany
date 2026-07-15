using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class PrintedCardRegistrationRepository(BingoDbContext db) : IPrintedCardRegistrationRepository
{
	public Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken) => db.Events.SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
	public Task<BingoCard?> GetCard(Guid eventId, string cardCode, CancellationToken cancellationToken) => db.Cards.SingleOrDefaultAsync(item => item.EventId == eventId && item.PublicCode == cardCode, cancellationToken);
	public void AddParticipant(Participant participant) => db.Participants.Add(participant);
	public void AddAuditEntry(AuditEntry entry) => db.AuditEntries.Add(entry);
	public Task SaveChanges(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
