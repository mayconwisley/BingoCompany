using System.Security.Cryptography;
using System.Text;
using BingoCompany.Domain;

namespace BingoCompany.Application;

public interface ICardGenerator { int[,] Generate(); }
public sealed class Bingo75CardGenerator : ICardGenerator
{
    public int[,] Generate()
    {
        var result = new int[5, 5];
        for (var column = 0; column < 5; column++)
        {
            var values = Enumerable.Range(column * 15 + 1, 15).OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).Take(5).Order().ToArray();
            for (var row = 0; row < 5; row++) result[row, column] = values[row];
        }
        result[2, 2] = 0; return result;
    }
}
public static class SecureDrawSequence
{
    public static int[] Generate() { var numbers = Enumerable.Range(1, 75).ToArray(); for (var i = numbers.Length - 1; i > 0; i--) { var j = RandomNumberGenerator.GetInt32(i + 1); (numbers[i], numbers[j]) = (numbers[j], numbers[i]); } return numbers; }
    public static string Hash(IEnumerable<int> sequence) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(',', sequence))));
}
public static class WinningPatternEvaluator
{
    public static bool IsCompleted(int[,] card, IReadOnlySet<int> drawn, WinningPattern pattern)
    {
        return RemainingNumbers(card, drawn, pattern) == 0;
    }
    public static int RemainingNumbers(int[,] card, IReadOnlySet<int> markedNumbers, WinningPattern pattern)
    {
        bool Marked(int number) => number == 0 || markedNumbers.Contains(number);
        int Missing(IEnumerable<(int Row, int Column)> positions) => positions.Count(position => !Marked(card[position.Row, position.Column]));
        var rows = Enumerable.Range(0, 5).Select(row => Missing(Enumerable.Range(0, 5).Select(column => (row, column)))).ToArray();
        return pattern switch
        {
            WinningPattern.HorizontalLine => rows.Min(),
            WinningPattern.TwoHorizontalLines => rows.Order().Take(2).Sum(),
            WinningPattern.FourCorners => Missing([(0, 0), (0, 4), (4, 0), (4, 4)]),
            WinningPattern.FullCard => Missing(Enumerable.Range(0, 5).SelectMany(row => Enumerable.Range(0, 5).Select(column => (row, column)))),
			WinningPattern.BColumn => Missing(Enumerable.Range(0, 5).Select(row => (row, 0))),
			WinningPattern.IColumn => Missing(Enumerable.Range(0, 5).Select(row => (row, 1))),
			WinningPattern.NColumn => Missing(Enumerable.Range(0, 5).Select(row => (row, 2))),
			WinningPattern.GColumn => Missing(Enumerable.Range(0, 5).Select(row => (row, 3))),
			WinningPattern.OColumn => Missing(Enumerable.Range(0, 5).Select(row => (row, 4))),
            _ => throw new InvalidOperationException("Regra de premiação inválida.")
        };
    }
}
