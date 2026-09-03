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

	public async Task<CompanyEventDetails?> Get(Guid eventId, int awardedCardsPage, int awardedCardsPageSize, CancellationToken cancellationToken)
	{
		var bingoEvent = await db.Events.AsNoTracking()
			.AsSplitQuery()
			.Include(item => item.Rounds)
			.ThenInclude(item => item.Stages)
			.SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
		if (bingoEvent is null) return null;
		var awardedCards = await GetAwardedCards(eventId, bingoEvent.Status, awardedCardsPage, awardedCardsPageSize, cancellationToken);
		var purchasedCardStatuses = await db.PurchasedDigitalCards(eventId).AsNoTracking().GroupBy(item => item.Status).Select(group => new { Status = group.Key, Count = group.Count() }).ToArrayAsync(cancellationToken);
		var purchasedCards = purchasedCardStatuses.Sum(item => item.Count);
		var activatedCards = purchasedCardStatuses.Where(item => item.Status == CardStatus.Active).Sum(item => item.Count);
		var awaitingActivationCards = purchasedCardStatuses.Where(item => item.Status == CardStatus.Assigned).Sum(item => item.Count);
		var waitlistEntries = await db.CardPurchaseWaitlistEntries.AsNoTracking().CountAsync(item => item.EventId == eventId, cancellationToken);
		var participants = await db.Participants.AsNoTracking().CountAsync(item => item.EventId == eventId, cancellationToken);
		var cards = await db.Cards.AsNoTracking().CountAsync(item => item.EventId == eventId, cancellationToken);
		var eligibleCards = await db.Cards.AsNoTracking().CountAsync(item => item.EventId == eventId && item.Status == CardStatus.Active && item.ParticipantId.HasValue, cancellationToken);
		var dashboard = bingoEvent.CardPurchaseLimit.HasValue
			? new CardPurchaseDashboard(bingoEvent.CardPurchaseLimit.Value, purchasedCards, activatedCards, activatedCards, awaitingActivationCards, waitlistEntries)
			: null;
		return new CompanyEventDetails(
			bingoEvent.Id,
			bingoEvent.Name,
			bingoEvent.PublicCode,
			bingoEvent.Status,
			bingoEvent.CardsPerParticipant,
			bingoEvent.IsCardPurchaseAvailableAt(DateTimeOffset.UtcNow),
			bingoEvent.CardPurchaseLimit,
			bingoEvent.CardPurchasePerParticipantLimit,
			bingoEvent.CardPurchaseClosesAt,
			bingoEvent.CardPurchaseLowStockThreshold,
			bingoEvent.CardPurchaseCancellationReason,
			bingoEvent.CardPurchaseLimit.HasValue ? Math.Max(0, bingoEvent.CardPurchaseLimit.Value - purchasedCards) : null,
			dashboard,
			participants,
			cards,
			eligibleCards,
			awardedCards,
			bingoEvent.Rounds
				.OrderBy(item => item.CreatedAt)
				.ThenBy(item => item.Sequence)
				.Select(item => new CompanyEventRound(
					item.Id,
					item.Name,
					item.Status,
					item.CreatedAt,
					item.Stages
						.OrderBy(stage => stage.Sequence)
						.Select(stage => new CompanyEventPrizeStage(
							stage.Sequence,
							stage.PrizeName,
							stage.Pattern,
							stage.PrizeImageDataUrl,
							stage.IsActive,
							stage.IsCompleted))
						.ToArray()))
				.ToArray());
	}

	private async Task<CompanyEventAwardedCardsPage> GetAwardedCards(Guid eventId, EventStatus eventStatus, int page, int pageSize, CancellationToken cancellationToken)
	{
		if (eventStatus != EventStatus.Finished) return new CompanyEventAwardedCardsPage([], page, pageSize, 0, 0);

		var query = from winner in db.RoundWinners.AsNoTracking()
					join card in db.Cards.AsNoTracking() on winner.CardId equals card.Id
					join participant in db.Participants.AsNoTracking() on winner.ParticipantId equals participant.Id
					join round in db.Rounds.AsNoTracking() on winner.RoundId equals round.Id
					join stage in db.PrizeStages.AsNoTracking() on winner.StageId equals stage.Id
					where winner.IsWinner && winner.RevealedAt.HasValue && round.EventId == eventId
					select new { winner, card, participant, round, stage };
		var totalItems = await query.CountAsync(cancellationToken);
		var items = await query
			.OrderBy(item => item.round.Sequence)
			.ThenBy(item => item.stage.Sequence)
			.ThenBy(item => item.winner.RevealedAt)
			.ThenBy(item => item.winner.Id)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(item => new CompanyEventAwardedCard(item.winner.Id, item.card.PublicCode, item.participant.Name, item.round.Name, item.stage.PrizeName))
			.ToArrayAsync(cancellationToken);
		return new CompanyEventAwardedCardsPage(items, page, pageSize, totalItems, (int)Math.Ceiling(totalItems / (double)pageSize));
	}
}
