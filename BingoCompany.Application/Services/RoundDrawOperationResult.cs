namespace BingoCompany.Application.Services;

public sealed record RoundDrawOperationResult(Guid RoundId, int Number, int Sequence, int WinnersDetected, bool TieBreakerRequired, BingoRoundStatistics Statistics);
