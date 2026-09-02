using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record CompanyEventPrizeStage(int Sequence, string PrizeName, WinningPattern Pattern, string? PrizeImageDataUrl, bool IsActive, bool IsCompleted);
