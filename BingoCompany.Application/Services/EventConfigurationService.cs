using BingoCompany.Application.Interfaces;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class EventConfigurationService(IEventConfigurationRepository repository) : IEventConfigurationService
{
	public async Task<EventCreationResult> Create(Guid companyId, string name, CardMarkingMode markingMode, CancellationToken cancellationToken)
	{
		var bingoEvent = new BingoEvent(companyId, name, markingMode: markingMode);
		repository.AddEvent(bingoEvent);
		repository.AddAuditEntry(new AuditEntry(bingoEvent.Id, "Evento criado", $"Evento {bingoEvent.Name} criado."));
		await repository.SaveChanges(cancellationToken);
		return new EventCreationResult(bingoEvent.Id, bingoEvent.Name, bingoEvent.PublicCode, bingoEvent.Status, bingoEvent.MarkingMode, bingoEvent.IsCardPurchaseOpen, bingoEvent.CreatedAt);
	}

	public async Task<bool> OpenRegistration(Guid eventId, int cardsPerParticipant, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		if (bingoEvent is null) return false;
		bingoEvent.OpenRegistration(cardsPerParticipant);
		repository.AddAuditEntry(new AuditEntry(eventId, "Inscrições públicas abertas", $"{cardsPerParticipant} cartela(s) por participante."));
		await repository.SaveChanges(cancellationToken);
		return true;
	}

	public async Task<bool> OpenCardPurchase(Guid eventId, int quantity, int perParticipantLimit, DateTimeOffset? closesAt, int lowStockThreshold, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		if (bingoEvent is null) return false;
		bingoEvent.OpenCardPurchase(quantity, perParticipantLimit, closesAt, lowStockThreshold);
		repository.AddAuditEntry(new AuditEntry(eventId, "Compra de cartelas aberta", $"Venda de até {quantity} cartelas digitais; máximo de {perParticipantLimit} por participante."));
		await repository.SaveChanges(cancellationToken);
		return true;
	}
}
