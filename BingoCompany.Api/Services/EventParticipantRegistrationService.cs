using BingoCompany.Api.Contracts;
using BingoCompany.Api.Interfaces;
using BingoCompany.Application;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Api.Services;

public sealed class EventParticipantRegistrationService(BingoDbContext db) : IEventParticipantRegistrationService
{
	public async Task<EventParticipantRegistration?> Register(Guid eventId, JoinEventRequest request, CancellationToken cancellationToken)
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

		var participant = new Participant(eventId, request.Name, request.Type, request.EmployeeRegistration, request.ResponsibleEmployeeName);
		var card = new BingoCard(eventId, participant.Id, CardType.Digital, new Bingo75CardGenerator().Generate());
		db.Participants.Add(participant);
		db.Cards.Add(card);
		db.AuditEntries.Add(new AuditEntry(eventId, "Cartela criada", $"Cartela digital {card.PublicCode} gerada."));
		await db.SaveChangesAsync(cancellationToken);

		return new EventParticipantRegistration(participant.Id, card.Id, card.PublicCode, ToRows(card.Numbers));
	}

	private static int[][] ToRows(int[,] card) => Enumerable.Range(0, 5)
		.Select(row => Enumerable.Range(0, 5).Select(column => card[row, column]).ToArray())
		.ToArray();
}
