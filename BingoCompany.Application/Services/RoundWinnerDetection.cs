using BingoCompany.Domain.Entities;

namespace BingoCompany.Application.Services;

public sealed record RoundWinnerDetection(RoundWinner? Winner)
{
	public bool HasWinner => Winner is not null;
}
