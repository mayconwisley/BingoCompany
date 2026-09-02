using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Queries;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class CompanyEventsReadRepository(BingoDbContext db) : ICompanyEventsReadRepository
{
	public async Task<CompanyEventsPage> List(Guid companyId, int page, int pageSize, CancellationToken cancellationToken)
	{
		var events = db.Events.AsNoTracking().Where(item => item.CompanyId == companyId).OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.Id);
		var totalItems = await events.CountAsync(cancellationToken);
		var items = await events.Skip((page - 1) * pageSize).Take(pageSize).Select(item => new CompanyEventListItem(item.Id, item.Name, item.PublicCode, item.Status, item.MarkingMode, item.IsCardPurchaseOpen, item.CreatedAt)).ToListAsync(cancellationToken);
		return new CompanyEventsPage(items, page, pageSize, totalItems, (int)Math.Ceiling(totalItems / (double)pageSize));
	}

	public async Task<CompanyEventDetails?> Get(Guid eventId, CancellationToken cancellationToken)
	{
		var bingoEvent = await db.Events.AsNoTracking().Include(item => item.Rounds).ThenInclude(item => item.Stages).Include(item => item.Cards).Include(item => item.Participants).SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
		if (bingoEvent is null) return null;
		var awardedCards = bingoEvent.Status != EventStatus.Finished ? [] : await (from winner in db.RoundWinners.AsNoTracking() join card in db.Cards.AsNoTracking() on winner.CardId equals card.Id join participant in db.Participants.AsNoTracking() on winner.ParticipantId equals participant.Id join round in db.Rounds.AsNoTracking() on winner.RoundId equals round.Id join stage in db.PrizeStages.AsNoTracking() on winner.StageId equals stage.Id where winner.IsWinner && winner.RevealedAt.HasValue && round.EventId == eventId orderby round.Sequence, stage.Sequence select new CompanyEventAwardedCard(card.PublicCode, participant.Name, round.Name, stage.PrizeName)).ToArrayAsync(cancellationToken);
		var purchasedCards = await db.Cards.Join(db.Participants, card => card.ParticipantId, participant => participant.Id, (card, participant) => new { card, participant }).CountAsync(item => item.card.EventId == eventId && item.card.Type == CardType.Digital && item.participant.ParticipantAccountId.HasValue && item.card.Status != CardStatus.Cancelled, cancellationToken);
		return new CompanyEventDetails(bingoEvent.Id, bingoEvent.Name, bingoEvent.PublicCode, bingoEvent.Status, bingoEvent.CardsPerParticipant, bingoEvent.IsCardPurchaseOpen, bingoEvent.CardPurchaseLimit, bingoEvent.CardPurchaseCancellationReason, bingoEvent.CardPurchaseLimit.HasValue ? Math.Max(0, bingoEvent.CardPurchaseLimit.Value - purchasedCards) : null, bingoEvent.Participants.Count, bingoEvent.Cards.Count, bingoEvent.Participants.OrderBy(item => item.Name).Select(item => new CompanyEventParticipant(item.Id, item.Name, item.Type)).ToArray(), bingoEvent.Cards.OrderByDescending(item => item.CreatedAt).Select(item => new CompanyEventCard(item.PublicCode, item.Type, item.Status, item.Fingerprint, item.ParticipantId)).ToArray(), awardedCards, bingoEvent.Rounds.OrderBy(item => item.CreatedAt).ThenBy(item => item.Sequence).Select(item => new CompanyEventRound(item.Id, item.Name, item.Status, item.CreatedAt, item.Stages.OrderBy(stage => stage.Sequence).Select(stage => new CompanyEventPrizeStage(stage.Sequence, stage.PrizeName, stage.Pattern, stage.PrizeImageDataUrl, stage.IsActive, stage.IsCompleted)).ToArray())).ToArray());
	}
}
