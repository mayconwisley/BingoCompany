using BingoCompany.Application.Interfaces;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class EventCardPurchaseService(IEventCardPurchaseRepository repository) : IEventCardPurchaseService
{
	public async Task<bool> UpdateLimit(Guid eventId, int quantity, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		if (bingoEvent is null) return false;

		var soldCards = await repository.CountActivePurchasedCards(eventId, cancellationToken);
		bingoEvent.UpdateCardPurchaseLimit(quantity, soldCards);
		repository.AddAuditEntry(new AuditEntry(eventId, "Venda de cartelas atualizada", $"Limite atualizado para {quantity} cartelas digitais."));
		await repository.SaveChanges(cancellationToken);
		return true;
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
