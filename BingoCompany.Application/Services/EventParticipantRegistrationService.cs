using BingoCompany.Application.Interfaces;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class EventParticipantRegistrationService(IEventParticipantRegistrationRepository repository) : IEventParticipantRegistrationService
{
	public async Task<EventParticipantRegistrationResult?> Register(Guid eventId, EventParticipantRegistrationRequest request, Guid? participantAccountId, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		return bingoEvent is null ? null : await Register(bingoEvent, request, participantAccountId, cancellationToken);
	}

	public async Task<EventParticipantRegistrationResult?> RegisterByPublicCode(string publicCode, EventParticipantRegistrationRequest request, Guid? participantAccountId, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEventByPublicCode(publicCode, cancellationToken);
		return bingoEvent is null ? null : await Register(bingoEvent, request, participantAccountId, cancellationToken);
	}

	private async Task<EventParticipantRegistrationResult> Register(BingoEvent bingoEvent, EventParticipantRegistrationRequest request, Guid? participantAccountId, CancellationToken cancellationToken)
	{
		var eventId = bingoEvent.Id;
		ValidateRegistrationAvailability(bingoEvent, participantAccountId);

		var participantData = await ResolveParticipant(request, participantAccountId, cancellationToken);
		var invitation = await ResolveInvitation(eventId, request.InvitationCode, participantAccountId, cancellationToken);
		var cardsQuantity = ResolveCardsQuantity(bingoEvent, request.CardsQuantity, invitation?.BonusCards ?? 0);
		await EnsureParticipantLimit(eventId, bingoEvent, participantAccountId, cardsQuantity, cancellationToken);
		await EnsureCardsAvailability(eventId, bingoEvent, cardsQuantity, cancellationToken);

		var participant = new Participant(eventId, participantData.Name, participantData.Type, participantData.EmployeeRegistration, participantData.ResponsibleEmployeeName, participantAccountId);
		var cards = CreateCards(eventId, participant.Id, cardsQuantity, bingoEvent.IsCardPurchaseOpen);
		repository.AddParticipant(participant);
		repository.AddCards(cards);
		invitation?.Redeem(participantAccountId!.Value);
		repository.AddAuditEntry(new AuditEntry(eventId, bingoEvent.IsCardPurchaseOpen ? "Cartelas digitais adquiridas" : "Cartelas digitais geradas", $"Lote com {cards.Length} cartela(s) digital(is) gerado para participante."));
		await repository.SaveChanges(cancellationToken);

		return new EventParticipantRegistrationResult(participant.Id, cards.Select(card => new EventParticipantCardResult(card.Id, card.PublicCode, ToRows(card.Numbers), card.Status)).ToArray());
	}

	private static void ValidateRegistrationAvailability(BingoEvent bingoEvent, Guid? participantAccountId)
	{
		if (bingoEvent.Status != EventStatus.RegistrationOpen) throw new InvalidOperationException("As inscrições estão fechadas.");
		if (bingoEvent.CardPurchaseCancellationReason is not null) throw new InvalidOperationException("A venda de cartelas deste evento foi encerrada.");
		if (bingoEvent.IsCardPurchaseOpen && !participantAccountId.HasValue) throw new UnauthorizedAccessException("Entre com sua conta de participante para comprar cartelas.");
		if (bingoEvent.IsCardPurchaseOpen && !bingoEvent.IsCardPurchaseAvailableAt(DateTimeOffset.UtcNow)) throw new InvalidOperationException("A venda de cartelas foi encerrada conforme o horário programado.");
	}

	private async Task<ParticipantData> ResolveParticipant(EventParticipantRegistrationRequest request, Guid? participantAccountId, CancellationToken cancellationToken)
	{
		if (participantAccountId.HasValue)
		{
			var account = await repository.GetParticipantAccount(participantAccountId.Value, cancellationToken);
			if (account is null) throw new UnauthorizedAccessException("A conta de participante não foi encontrada.");
			return new ParticipantData(account.Name, ParticipantType.Employee, null, null);
		}

		if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Informe seu nome para gerar uma cartela.");
		return new ParticipantData(request.Name, request.Type ?? ParticipantType.Employee, request.EmployeeRegistration, request.ResponsibleEmployeeName);
	}

	private async Task EnsureCardsAvailability(Guid eventId, BingoEvent bingoEvent, int cardsQuantity, CancellationToken cancellationToken)
	{
		if (!bingoEvent.CardPurchaseLimit.HasValue) return;
		var soldCards = await repository.CountActivePurchasedCards(eventId, cancellationToken);
		if (soldCards + cardsQuantity > bingoEvent.CardPurchaseLimit.Value) throw new InvalidOperationException("Não há cartelas suficientes disponíveis para esta compra.");
	}

	private async Task EnsureParticipantLimit(Guid eventId, BingoEvent bingoEvent, Guid? participantAccountId, int cardsQuantity, CancellationToken cancellationToken)
	{
		if (!bingoEvent.IsCardPurchaseOpen || !participantAccountId.HasValue) return;
		var participantLimit = bingoEvent.CardPurchasePerParticipantLimit ?? 5;
		var acquiredCards = await repository.CountPurchasedCards(eventId, participantAccountId.Value, cancellationToken);
		if (acquiredCards + cardsQuantity > participantLimit)
			throw new InvalidOperationException($"Cada participante pode adquirir no máximo {participantLimit} cartela(s) neste evento.");
	}

	private async Task<CardPurchaseInvitation?> ResolveInvitation(Guid eventId, string? invitationCode, Guid? participantAccountId, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(invitationCode)) return null;
		if (!participantAccountId.HasValue) throw new UnauthorizedAccessException("Entre com sua conta de participante para usar um convite.");
		var invitation = await repository.GetInvitation(eventId, invitationCode.Trim().ToUpperInvariant(), cancellationToken);
		if (invitation is null || !invitation.IsAvailableAt(DateTimeOffset.UtcNow)) throw new InvalidOperationException("Este convite não está disponível.");
		return invitation;
	}

	private static BingoCard[] CreateCards(Guid eventId, Guid participantId, int cardsQuantity, bool cardPurchaseOpen)
	{
		return Enumerable.Range(0, cardsQuantity)
			.Select(_ => new BingoCard(eventId, participantId, CardType.Digital, new Bingo75CardGenerator().Generate(), activateImmediately: !cardPurchaseOpen))
			.ToArray();
	}

	private static int[][] ToRows(int[,] card) => Enumerable.Range(0, 5).Select(row => Enumerable.Range(0, 5).Select(column => card[row, column]).ToArray()).ToArray();

	private static int ResolveCardsQuantity(BingoEvent bingoEvent, int? requestedQuantity, int invitationBonusCards)
	{
		if (!bingoEvent.IsCardPurchaseOpen) return Math.Max(1, bingoEvent.CardsPerParticipant);
		var quantity = requestedQuantity ?? 1;
		if (quantity is < 1 or > 100) throw new InvalidOperationException("Informe entre 1 e 100 cartelas digitais.");
		return quantity + invitationBonusCards;
	}

	private sealed record ParticipantData(string Name, ParticipantType Type, string? EmployeeRegistration, string? ResponsibleEmployeeName);
}
