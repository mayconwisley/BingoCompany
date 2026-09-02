namespace BingoCompany.Application.Services;

public sealed record ManualCardMarkingResult(bool Exists, bool WinnerDetected, Guid? RoundId);
