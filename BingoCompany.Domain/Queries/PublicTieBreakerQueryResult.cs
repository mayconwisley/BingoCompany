namespace BingoCompany.Domain.Queries;

public sealed record PublicTieBreakerQueryResult(string ParticipantName, int? Number, bool IsWinner);
