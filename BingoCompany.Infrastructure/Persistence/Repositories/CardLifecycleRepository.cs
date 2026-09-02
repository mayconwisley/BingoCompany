using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class CardLifecycleRepository(BingoDbContext db) : ICardLifecycleRepository
{
	public Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken) => db.Events.SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
	public Task<BingoCard?> GetCard(Guid eventId, string cardCode, CancellationToken cancellationToken) => db.Cards.SingleOrDefaultAsync(item => item.EventId == eventId && item.PublicCode == cardCode, cancellationToken);
	public Task<BingoRound?> GetLatestCompletedRound(Guid eventId, CancellationToken cancellationToken) => db.Rounds.Where(item => item.EventId == eventId && (item.Status == RoundStatus.Finished || item.Status == RoundStatus.Cancelled)).OrderByDescending(item => item.Sequence).FirstOrDefaultAsync(cancellationToken);
	public Task<bool> HasStartedRoundAfter(Guid eventId, int sequence, CancellationToken cancellationToken) => db.Rounds.AnyAsync(item => item.EventId == eventId && item.Sequence > sequence && (item.Status == RoundStatus.Drawing || item.Status == RoundStatus.WinnerDetected || item.Status == RoundStatus.TieBreaker), cancellationToken);
	public Task<bool> WasEligible(Guid roundId, Guid cardId, CancellationToken cancellationToken) => db.RoundEligibleCards.AnyAsync(item => item.RoundId == roundId && item.CardId == cardId, cancellationToken);
	public Task<string?> GetCompanyName(Guid eventId, CancellationToken cancellationToken) => db.Events.Where(item => item.Id == eventId).Join(db.Companies, bingoEvent => bingoEvent.CompanyId, company => company.Id, (_, company) => company.Name).SingleOrDefaultAsync(cancellationToken);
	public async Task<IReadOnlyCollection<BingoCard>> GetPrintedCards(Guid eventId, CancellationToken cancellationToken) => await db.Cards.Where(item => item.EventId == eventId && item.Type == CardType.Printed).OrderBy(item => item.CreatedAt).ToListAsync(cancellationToken);
	public Task<Participant?> GetParticipant(Guid eventId, Guid participantId, CancellationToken cancellationToken) => db.Participants.SingleOrDefaultAsync(item => item.Id == participantId && item.EventId == eventId, cancellationToken);
	public void AddCard(BingoCard card) => db.Cards.Add(card);
	public void AddCards(IReadOnlyCollection<BingoCard> cards) => db.Cards.AddRange(cards);
	public void AddAuditEntry(AuditEntry entry) => db.AuditEntries.Add(entry);
	public Task SaveChanges(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
