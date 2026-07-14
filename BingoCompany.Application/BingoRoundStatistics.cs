using BingoCompany.Domain.Entities;

namespace BingoCompany.Application;

public sealed record BingoRoundStatistics(int TotalCards, int OneNumberAway, int TwoNumbersAway, int ThreeNumbersAway, int AwardedCards);

public static class BingoRoundStatisticsCalculator
{
    public static BingoRoundStatistics Calculate(IEnumerable<BingoCard> cards, Func<BingoCard, IReadOnlySet<int>> markedNumbers, WinningPattern pattern)
    {
        var remaining = cards.Select(card => WinningPatternEvaluator.RemainingNumbers(card.Numbers, markedNumbers(card), pattern)).ToArray();
        return new BingoRoundStatistics(remaining.Length, remaining.Count(value => value == 1), remaining.Count(value => value == 2), remaining.Count(value => value == 3), remaining.Count(value => value == 0));
    }
}
