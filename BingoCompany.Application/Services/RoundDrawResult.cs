using BingoCompany.Domain.Entities;

namespace BingoCompany.Application.Services;

public sealed record RoundDrawResult(DrawnNumber DrawnNumber, IReadOnlyCollection<RoundWinner> Winners)
{
	public bool HasWinners => Winners.Count > 0;
	public bool RequiresTieBreaker => Winners.Count > 1;
}
