using BingoCompany.Domain.Enums;

namespace BingoCompany.Application.Services;

public sealed record PrizeStageOperationResult(Guid RoundId, string? CurrentPrize, RoundStatus Status);
