using BingoCompany.Domain.Entities;

namespace BingoCompany.Application.Services;

public sealed record WinnerRevealResult(RoundWinner Winner, bool TieBreakerApplied);
