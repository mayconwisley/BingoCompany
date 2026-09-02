using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record PublicWinnerQueryResult(string ParticipantName, string PrizeName, WinningPattern Pattern, string? PrizeImageDataUrl, bool IsPrizeDeliveryPending, IReadOnlyCollection<PublicTieBreakerQueryResult> TieBreakers);
