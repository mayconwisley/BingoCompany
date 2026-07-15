namespace BingoCompany.Api.Services;

public sealed record EventParticipantRegistration(Guid ParticipantId, IReadOnlyCollection<EventParticipantCard> Cards);

public sealed record EventParticipantCard(Guid CardId, string PublicCode, int[][] Numbers, CardStatus Status);
