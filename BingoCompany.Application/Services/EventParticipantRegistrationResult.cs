namespace BingoCompany.Application.Services;

public sealed record EventParticipantRegistrationResult(Guid ParticipantId, IReadOnlyCollection<EventParticipantCardResult> Cards);
