using BingoCompany.Application.Interfaces;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class EventCardPurchaseService(IEventCardPurchaseRepository repository) : IEventCardPurchaseService
{
	public async Task<bool> UpdateSettings(Guid eventId, int quantity, int perParticipantLimit, DateTimeOffset? closesAt, int lowStockThreshold, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		if (bingoEvent is null) return false;

		var soldCards = await repository.CountActivePurchasedCards(eventId, cancellationToken);
		bingoEvent.UpdateCardPurchaseSettings(quantity, perParticipantLimit, closesAt, lowStockThreshold, soldCards);
		repository.AddAuditEntry(new AuditEntry(eventId, "Venda de cartelas atualizada", $"Limite: {quantity}; máximo por participante: {perParticipantLimit}; encerramento: {closesAt?.ToString("O") ?? "não programado"}."));
		await repository.SaveChanges(cancellationToken);
		return true;
	}

	public async Task<CardPurchaseInvitationResult?> CreateInvitation(Guid eventId, int bonusCards, DateTimeOffset? expiresAt, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		if (bingoEvent is null) return null;
		if (!bingoEvent.IsCardPurchaseAvailableAt(DateTimeOffset.UtcNow)) throw new InvalidOperationException("A venda de cartelas não está aberta para criar convites.");

		var invitation = new CardPurchaseInvitation(eventId, bonusCards, expiresAt);
		repository.AddInvitation(invitation);
		repository.AddAuditEntry(new AuditEntry(eventId, "Convite promocional criado", $"Convite com {bonusCards} cartela(s) de bônus criado."));
		await repository.SaveChanges(cancellationToken);
		return new CardPurchaseInvitationResult(invitation.Code, invitation.BonusCards, invitation.ExpiresAt);
	}

	public async Task<CardPurchaseCancellationResult?> Cancel(Guid eventId, string reason, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		if (bingoEvent is null) return null;

		bingoEvent.CancelCardPurchase(reason);
		var purchasedCards = await repository.GetPurchasedCards(eventId, cancellationToken);
		foreach (var card in purchasedCards) card.InvalidateForCancelledSale(bingoEvent.CardPurchaseCancellationReason!);
		repository.AddAuditEntry(new AuditEntry(eventId, "Venda de cartelas cancelada", $"Venda cancelada: {bingoEvent.CardPurchaseCancellationReason}. {purchasedCards.Count} cartela(s) invalidada(s)."));
		await repository.SaveChanges(cancellationToken);
		return new CardPurchaseCancellationResult(purchasedCards.Count);
	}
}
