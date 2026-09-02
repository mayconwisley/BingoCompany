namespace BingoCompany.Application.Services;

public sealed record WinnerTieBreakerResult(string ParticipantName, int? TieBreakerNumber, bool IsWinner);
