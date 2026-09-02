using BingoCompany.Domain.Enums;

namespace BingoCompany.Application.Services;

public sealed record RoundStageDefinition(string PrizeName, WinningPattern Pattern, string? PrizeImageDataUrl);
