namespace BingoCompany.Domain;

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
		WinningPattern.BDiagonal => 7,
		WinningPattern.ODiagonal => 8,
		WinningPattern.HorizontalLine => 9,
		WinningPattern.XPattern => 10,
		WinningPattern.TPattern => 11,
		WinningPattern.Cross => 12,
		WinningPattern.TwoHorizontalLines => 13,
		WinningPattern.Frame => 14,
		WinningPattern.FullCard => 15,
		_ => throw new InvalidOperationException("Regra de premiação inválida.")
	};
}
