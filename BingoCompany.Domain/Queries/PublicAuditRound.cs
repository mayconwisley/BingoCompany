using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record PublicAuditRound(string Name, int Sequence, RoundStatus Status, string? SequenceHash, IReadOnlyCollection<int>? FullSequence, IReadOnlyCollection<PublicAuditDrawnNumber> DrawnNumbers, IReadOnlyCollection<PublicAuditStage> Stages, IReadOnlyCollection<PublicAuditWinner> Winners);
