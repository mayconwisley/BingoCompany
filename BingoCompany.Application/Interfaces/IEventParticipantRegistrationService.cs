using BingoCompany.Application.Services;

namespace BingoCompany.Application.Interfaces;

public interface IEventParticipantRegistrationService
{
	Task<EventParticipantRegistrationResult?> Register(Guid eventId, EventParticipantRegistrationRequest request, Guid? participantAccountId, CancellationToken cancellationToken);
	Task<EventParticipantRegistrationResult?> RegisterByPublicCode(string publicCode, EventParticipantRegistrationRequest request, Guid? participantAccountId, CancellationToken cancellationToken);
}
