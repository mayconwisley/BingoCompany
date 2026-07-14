using System.Security.Cryptography;

namespace BingoCompany.Application;

public static class SecureTieBreakerSequence
{
	public static int[] Generate(int candidatesCount)
	{
		if (candidatesCount < 1) throw new ArgumentOutOfRangeException(nameof(candidatesCount), "O desempate precisa de ao menos um candidato.");

		var positions = Enumerable.Range(1, candidatesCount).ToArray();
		for (var index = positions.Length - 1; index > 0; index--)
		{
			var randomIndex = RandomNumberGenerator.GetInt32(index + 1);
			(positions[index], positions[randomIndex]) = (positions[randomIndex], positions[index]);
		}

		return positions;
	}
}
