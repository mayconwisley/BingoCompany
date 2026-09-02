using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record PublicRoundQueryResult(
	Guid Id,
	string Name,
	int Sequence,
	RoundStatus Status,
	string? SequenceHash,
	string? CurrentPrize,
	string? CurrentPrizeImageDataUrl,
	string? PresentationPrizeImageDataUrl,
	int EligibleCards,
	IReadOnlyCollection<PublicPrizeStageQueryResult> Stages,
	IReadOnlyCollection<int> DrawnNumbers,
	int WinnerDetectedCount,
	bool TieBreakerRequired,
	bool HasPrizeDeliveryPending,
	PublicRoundStatisticsQueryResult? Statistics,
	PublicWinnerQueryResult? Winner);
