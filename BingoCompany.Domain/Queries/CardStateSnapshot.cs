using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record CardStateSnapshot(
	Guid Id,
	string PublicCode,
	string? ParticipantName,
	string? ResponsibleEmployeeName,
	bool IsWinner,
	int[][] Numbers,
	EventStatus EventStatus,
	CardMarkingMode MarkingMode,
	Guid? RoundId,
	RoundStatus? RoundStatus,
	string? CurrentPrize,
	WinningPattern? CurrentPattern,
	int? RemainingNumbersToWin,
	IReadOnlyCollection<int> DrawnNumbers,
	IReadOnlyCollection<int> MarkedNumbers,
	int LastSequence,
	bool CanGenerateNextCard);
