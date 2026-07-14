namespace BingoCompany.Api.Services;

public sealed record EventParticipantRegistration(Guid ParticipantId, Guid CardId, string PublicCode, int[][] Numbers);
