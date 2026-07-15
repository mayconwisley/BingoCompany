using BingoCompany.Api.Contracts;
using BingoCompany.Api.Services;

namespace BingoCompany.Api.Interfaces;

public interface IEventParticipantRegistrationService
{
	Task<EventParticipantRegistration?> Register(Guid eventId, JoinEventRequest request, Guid? participantAccountId, CancellationToken cancellationToken);
}
