using BingoCompany.Application.Interfaces;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class PublicCardActivationService(IPublicCardActivationRepository repository) : IPublicCardActivationService
{
	public async Task<DigitalCardActivationResult> Activate(string publicCode, string cardCode, Guid? participantAccountId, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(publicCode, cancellationToken);
		if (bingoEvent is null) return DigitalCardActivationResult.EventNotFound;
		if (bingoEvent.Status is not (EventStatus.RegistrationOpen or EventStatus.Running)) throw new InvalidOperationException("Este evento não aceita mais ativação de cartelas.");

		var card = await repository.GetCard(bingoEvent.Id, cardCode, cancellationToken);
		if (card is null) return DigitalCardActivationResult.CardNotFound;
		if (card.Type != CardType.Digital) throw new InvalidOperationException("Apenas cartelas digitais podem ser ativadas por este acesso.");
		await EnsureParticipantOwnsCard(card, participantAccountId, cancellationToken);
		if (card.Status == CardStatus.Active) return DigitalCardActivationResult.AlreadyActive;
		if (card.Status != CardStatus.Assigned) throw new InvalidOperationException("Esta cartela não está disponível para ativação.");

		card.Activate();
		repository.AddAuditEntry(new AuditEntry(bingoEvent.Id, "Cartela digital ativada", $"Cartela digital {card.PublicCode} ativada pelo participante."));
		await repository.SaveChanges(cancellationToken);
		return DigitalCardActivationResult.Activated;
	}

	private async Task EnsureParticipantOwnsCard(BingoCard card, Guid? participantAccountId, CancellationToken cancellationToken)
	{
		if (!card.ParticipantId.HasValue) return;
		var cardAccountId = await repository.GetParticipantAccountId(card.ParticipantId.Value, cancellationToken);
		if (cardAccountId.HasValue && cardAccountId != participantAccountId) throw new UnauthorizedAccessException();
	}
}
