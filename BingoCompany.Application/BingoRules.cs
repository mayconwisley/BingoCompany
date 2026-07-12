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
        bool Marked(int n) => n == 0 || drawn.Contains(n);
        var rows = Enumerable.Range(0, 5).Select(r => Enumerable.Range(0, 5).All(c => Marked(card[r, c]))).ToArray();
        return pattern switch
        {
            WinningPattern.HorizontalLine => rows.Any(x => x),
            WinningPattern.TwoHorizontalLines => rows.Count(x => x) >= 2,
            WinningPattern.FourCorners => Marked(card[0, 0]) && Marked(card[0, 4]) && Marked(card[4, 0]) && Marked(card[4, 4]),
            WinningPattern.FullCard => Enumerable.Range(0, 5).All(r => Enumerable.Range(0, 5).All(c => Marked(card[r, c]))),
			WinningPattern.MainDiagonal => Enumerable.Range(0, 5).All(index => Marked(card[index, index])),
			WinningPattern.SecondaryDiagonal => Enumerable.Range(0, 5).All(index => Marked(card[index, 4 - index])),
			WinningPattern.BColumn => Enumerable.Range(0, 5).All(row => Marked(card[row, 0])),
			WinningPattern.OColumn => Enumerable.Range(0, 5).All(row => Marked(card[row, 4])),
            _ => false
        };
    }
}
