using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

internal static class CardPurchaseQueries
{
	public static IQueryable<BingoCard> PurchasedDigitalCards(this BingoDbContext db, Guid eventId)
	{
		return from card in db.Cards
			join participant in db.Participants on card.ParticipantId equals participant.Id
			where card.EventId == eventId
				&& card.Type == CardType.Digital
				&& participant.ParticipantAccountId.HasValue
				&& card.Status != CardStatus.Cancelled
			select card;
	}

	public static IQueryable<BingoCard> PurchasedDigitalCards(this BingoDbContext db, Guid eventId, Guid participantAccountId)
	{
		return from card in db.Cards
			join participant in db.Participants on card.ParticipantId equals participant.Id
			where card.EventId == eventId
				&& card.Type == CardType.Digital
				&& participant.ParticipantAccountId == participantAccountId
				&& card.Status != CardStatus.Cancelled
			select card;
	}
}
