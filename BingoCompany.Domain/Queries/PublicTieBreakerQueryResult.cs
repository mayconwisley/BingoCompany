namespace BingoCompany.Domain.Queries;

public sealed record PublicTieBreakerQueryResult(string ParticipantName, string CardCode, int? Number, bool IsWinner);
