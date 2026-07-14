using BingoCompany.Domain;

namespace BingoCompany.Api.Contracts;

public sealed record CreatePrizeStageRequest(int Sequence, string PrizeName, WinningPattern Pattern, string? PrizeImageDataUrl = null);
