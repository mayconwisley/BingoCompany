using BingoCompany.Domain.Enums;

namespace BingoCompany.Application.Services;

public sealed record WinnerRevealOperationResult(string ParticipantName, string CardCode, string Prize, IReadOnlyCollection<WinnerTieBreakerResult> TieBreakers, string? CurrentPrize, RoundStatus Status);
