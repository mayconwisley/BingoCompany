namespace BingoCompany.Domain.Queries;

public sealed record PublicAuditWinner(string ParticipantName, string CardCode, string PrizeName, bool IsWinner, int? TieBreakerNumber, DateTimeOffset DetectedAt, DateTimeOffset? RevealedAt);
