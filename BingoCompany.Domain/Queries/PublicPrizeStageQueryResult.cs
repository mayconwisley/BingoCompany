using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record PublicPrizeStageQueryResult(string PrizeName, WinningPattern Pattern, string? PrizeImageDataUrl, bool IsActive, bool IsCompleted);
