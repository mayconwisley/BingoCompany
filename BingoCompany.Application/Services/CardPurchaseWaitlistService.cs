using BingoCompany.Application.Interfaces;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class CardPurchaseWaitlistService(ICardPurchaseWaitlistRepository repository) : ICardPurchaseWaitlistService
{
	public async Task<CardPurchaseWaitlistResult?> Join(Guid participantAccountId, string publicCode, int requestedQuantity, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEventByPublicCode(publicCode, cancellationToken);
		if (bingoEvent is null) return null;
		if (!bingoEvent.IsCardPurchaseAvailableAt(DateTimeOffset.UtcNow)) throw new InvalidOperationException("A lista de espera não está disponível porque a venda foi encerrada.");
		if (requestedQuantity is < 1 or > 100) throw new InvalidOperationException("Informe entre 1 e 100 cartelas para a lista de espera.");
		var participantLimit = bingoEvent.CardPurchasePerParticipantLimit ?? 5;
		if (requestedQuantity > participantLimit)
			throw new InvalidOperationException($"A lista de espera permite no máximo {participantLimit} cartela(s) por participante.");
		var purchasedCards = await repository.CountActivePurchasedCards(bingoEvent.Id, cancellationToken);
		if (!bingoEvent.CardPurchaseLimit.HasValue || purchasedCards < bingoEvent.CardPurchaseLimit.Value) throw new InvalidOperationException("Ainda há cartelas disponíveis para reserva.");

		var existingEntry = await repository.GetEntry(bingoEvent.Id, participantAccountId, cancellationToken);
		if (existingEntry is not null)
			return new CardPurchaseWaitlistResult(await GetPosition(bingoEvent.Id, existingEntry.CreatedAt, cancellationToken), existingEntry.RequestedQuantity, true);

		var entry = new CardPurchaseWaitlistEntry(bingoEvent.Id, participantAccountId, requestedQuantity);
		repository.Add(entry);
		repository.AddAuditEntry(new AuditEntry(bingoEvent.Id, "Lista de espera", $"Participante entrou na lista de espera para {requestedQuantity} cartela(s)."));
		await repository.SaveChanges(cancellationToken);
		return new CardPurchaseWaitlistResult(await GetPosition(bingoEvent.Id, entry.CreatedAt, cancellationToken), requestedQuantity, false);
	}

	private async Task<int> GetPosition(Guid eventId, DateTimeOffset createdAt, CancellationToken cancellationToken) => await repository.CountEntriesBefore(eventId, createdAt, cancellationToken) + 1;
}
