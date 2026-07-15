using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Queries;
using BingoCompany.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence.Repositories;

public sealed class ParticipantCardsRepository(BingoDbContext db) : IParticipantCardsRepository
{
	public async Task<IReadOnlyCollection<ParticipantCardSummary>> GetActiveCards(Guid participantAccountId, CancellationToken cancellationToken) => await CreateCardsQuery(participantAccountId, isActive: true).ToListAsync(cancellationToken);

	public Task<int> CountHistoryCards(Guid participantAccountId, CancellationToken cancellationToken) => CreateCardsQuery(participantAccountId, isActive: false).CountAsync(cancellationToken);

	public async Task<IReadOnlyCollection<ParticipantCardSummary>> GetHistoryCards(Guid participantAccountId, int skip, int take, CancellationToken cancellationToken) => await CreateCardsQuery(participantAccountId, isActive: false).Skip(skip).Take(take).ToListAsync(cancellationToken);

	private IQueryable<ParticipantCardSummary> CreateCardsQuery(Guid participantAccountId, bool isActive) =>
		from card in db.Cards.AsNoTracking()
		join participant in db.Participants.AsNoTracking() on card.ParticipantId equals participant.Id
		join bingoEvent in db.Events.AsNoTracking() on card.EventId equals bingoEvent.Id
		where participant.ParticipantAccountId == participantAccountId && (isActive ? card.Status == CardStatus.Active : card.Status != CardStatus.Active)
		orderby bingoEvent.CreatedAt descending, card.CreatedAt descending
		select new ParticipantCardSummary(bingoEvent.Id, bingoEvent.PublicCode, bingoEvent.Name, bingoEvent.Status, card.PublicCode, card.Status, card.InvalidationReason, bingoEvent.CardPurchaseCancellationReason, card.CreatedAt);
}
