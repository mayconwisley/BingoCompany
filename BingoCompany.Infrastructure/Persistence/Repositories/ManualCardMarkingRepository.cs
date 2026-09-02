using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class ManualCardMarkingRepository(BingoDbContext db) : IManualCardMarkingRepository
{
	public Task<BingoEvent?> GetEvent(Guid eventId, CancellationToken cancellationToken) => db.Events.SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
	public Task<BingoCard?> GetCard(Guid eventId, string cardCode, CancellationToken cancellationToken) => db.Cards.SingleOrDefaultAsync(item => item.EventId == eventId && item.PublicCode == cardCode, cancellationToken);
	public Task<BingoRound?> GetDrawingRound(Guid eventId, CancellationToken cancellationToken) => db.Rounds.Include(item => item.Stages).Include(item => item.DrawnNumbers).Where(item => item.EventId == eventId && item.Status == RoundStatus.Drawing).OrderByDescending(item => item.Sequence).FirstOrDefaultAsync(cancellationToken);
	public Task<bool> HasMark(Guid roundId, Guid cardId, int number, CancellationToken cancellationToken) => db.CardMarks.AnyAsync(item => item.RoundId == roundId && item.CardId == cardId && item.Number == number, cancellationToken);
	public async Task<IReadOnlyCollection<int>> GetMarkedNumbers(Guid roundId, Guid cardId, CancellationToken cancellationToken) => await db.CardMarks.Where(item => item.RoundId == roundId && item.CardId == cardId).Select(item => item.Number).ToListAsync(cancellationToken);
	public Task<bool> IsExcludedFromActiveStage(Guid roundId, Guid stageId, Guid cardId, CancellationToken cancellationToken) => db.RoundWinners.AnyAsync(item => item.RoundId == roundId && item.StageId == stageId && item.CardId == cardId, cancellationToken);
	public void AddMark(CardMark mark) => db.CardMarks.Add(mark);
	public void AddWinner(RoundWinner winner) => db.RoundWinners.Add(winner);
	public void AddAuditEntry(AuditEntry entry) => db.AuditEntries.Add(entry);
	public Task SaveChanges(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
