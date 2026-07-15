using BingoCompany.Application.Interfaces;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class PrintedCardRegistrationService(IPrintedCardRegistrationRepository repository) : IPrintedCardRegistrationService
{
	public async Task<PrintedCardRegistrationResult?> Register(Guid eventId, string cardCode, PrintedCardParticipantRegistration registration, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		var card = await repository.GetCard(eventId, cardCode, cancellationToken);
		if (bingoEvent is null || card is null) return null;
		if (bingoEvent.Status == EventStatus.Finished) throw new InvalidOperationException("O evento encerrado não pode ser alterado.");
		if (card.Type != CardType.Printed || card.Status != CardStatus.Printed) throw new InvalidOperationException("Esta cartela impressa já foi associada.");
		if (string.IsNullOrWhiteSpace(registration.Name)) throw new InvalidOperationException("Informe o nome do participante.");
		if (registration.Type != ParticipantType.Employee && string.IsNullOrWhiteSpace(registration.ResponsibleEmployeeName)) throw new InvalidOperationException("Informe o colaborador responsável.");

		var participant = new Participant(eventId, registration.Name, registration.Type, registration.EmployeeRegistration, registration.ResponsibleEmployeeName);
		card.Assign(participant.Id);
		card.Activate();
		repository.AddParticipant(participant);
		repository.AddAuditEntry(new AuditEntry(eventId, "Cartela impressa registrada", $"Cartela {card.PublicCode} associada e ativada para {participant.Name}."));
		await repository.SaveChanges(cancellationToken);
		return new PrintedCardRegistrationResult(card.PublicCode, participant.Name);
	}
}
