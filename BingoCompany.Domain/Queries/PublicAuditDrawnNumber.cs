namespace BingoCompany.Domain.Queries;

public sealed record PublicAuditDrawnNumber(int Number, int Sequence, DateTimeOffset DrawnAt);
