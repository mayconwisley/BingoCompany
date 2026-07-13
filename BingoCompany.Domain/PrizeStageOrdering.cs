namespace BingoCompany.Domain.Models;

public static class PrizeStageOrdering
{
	public static IReadOnlyList<T> Order<T>(IEnumerable<T> stages, Func<T, WinningPattern> patternSelector)
	{
		return stages
			.OrderBy(stage => GetPriority(patternSelector(stage)))
			.ToArray();
	}

	private static int GetPriority(WinningPattern pattern) => pattern switch
	{
		WinningPattern.BColumn => 1,
		WinningPattern.IColumn => 2,
		WinningPattern.NColumn => 3,
		WinningPattern.GColumn => 4,
		WinningPattern.OColumn => 5,
		WinningPattern.FourCorners => 6,
		WinningPattern.HorizontalLine => 7,
		WinningPattern.TwoHorizontalLines => 8,
		WinningPattern.FullCard => 9,
		_ => throw new InvalidOperationException("Regra de premiação inválida.")
	};
}
