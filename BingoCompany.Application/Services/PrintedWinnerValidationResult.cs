namespace BingoCompany.Application.Services;

public sealed record PrintedWinnerValidationResult(string ParticipantName, string CardCode, int CandidatesCount, bool TieBreakerRequired);
