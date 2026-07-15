using BingoCompany.Api.Contracts;
using BingoCompany.Api.Interfaces;
using BingoCompany.Application;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Api.Services;

public sealed class EventParticipantRegistrationService(BingoDbContext db) : IEventParticipantRegistrationService
{
	public async Task<EventParticipantRegistration?> Register(Guid eventId, JoinEventRequest request, Guid? participantAccountId, CancellationToken cancellationToken)
	{
		var bingoEvent = await db.Events.SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
		if (bingoEvent is null)
		{
			return null;
		}

		if (bingoEvent.Status != EventStatus.RegistrationOpen)
		{
			throw new InvalidOperationException("As inscrições estão fechadas.");
		}
		if (bingoEvent.IsCardPurchaseOpen && !participantAccountId.HasValue)
			throw new UnauthorizedAccessException("Entre com sua conta de participante para comprar cartelas.");

		var participantName = request.Name;
		if (participantAccountId.HasValue)
		{
			var account = await db.ParticipantAccounts.SingleOrDefaultAsync(item => item.Id == participantAccountId.Value, cancellationToken);
			if (account is null) throw new UnauthorizedAccessException("A conta de participante não foi encontrada.");
			participantName = account.Name;
		}
		var participant = new Participant(eventId, participantName, request.Type, request.EmployeeRegistration, request.ResponsibleEmployeeName, participantAccountId);
		var cardsQuantity = ResolveCardsQuantity(bingoEvent, request.CardsQuantity);
		if (bingoEvent.CardPurchaseLimit.HasValue)
		{
			var soldCards = await db.Cards.Join(db.Participants, card => card.ParticipantId, participant => participant.Id, (card, participant) => new { card, participant })
			.CountAsync(item => item.card.EventId == eventId && item.card.Type == CardType.Digital && item.participant.ParticipantAccountId.HasValue && item.card.Status != CardStatus.Cancelled, cancellationToken);
			if (soldCards + cardsQuantity > bingoEvent.CardPurchaseLimit.Value) throw new InvalidOperationException("Não há cartelas suficientes disponíveis para esta compra.");
		}
		var cards = Enumerable.Range(0, cardsQuantity)
			.Select(_ => new BingoCard(
				eventId,
				participant.Id,
				CardType.Digital,
				new Bingo75CardGenerator().Generate(),
				activateImmediately: !bingoEvent.IsCardPurchaseOpen && cardsQuantity == 1))
			.ToArray();
		db.Participants.Add(participant);
		db.Cards.AddRange(cards);
		var auditAction = bingoEvent.IsCardPurchaseOpen ? "Cartelas digitais adquiridas" : "Cartelas digitais geradas";
		db.AuditEntries.Add(new AuditEntry(eventId, auditAction, $"Lote com {cards.Length} cartela(s) digital(is) gerado para participante."));
		await db.SaveChangesAsync(cancellationToken);

		return new EventParticipantRegistration(participant.Id, cards
			.Select(card => new EventParticipantCard(card.Id, card.PublicCode, ToRows(card.Numbers), card.Status))
			.ToArray());
	}

	private static int[][] ToRows(int[,] card) => Enumerable.Range(0, 5)
		.Select(row => Enumerable.Range(0, 5).Select(column => card[row, column]).ToArray())
		.ToArray();

	private static int ResolveCardsQuantity(BingoEvent bingoEvent, int? requestedQuantity)
	{
		if (!bingoEvent.IsCardPurchaseOpen)
			return Math.Max(1, bingoEvent.CardsPerParticipant);
		var quantity = requestedQuantity ?? 1;
		if (quantity < 1 || quantity > 100)
			throw new InvalidOperationException("Informe entre 1 e 100 cartelas digitais.");

		return quantity;
	}
}
