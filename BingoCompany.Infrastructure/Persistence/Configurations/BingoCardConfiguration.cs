using System.Text.Json;
using BingoCompany.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BingoCompany.Infrastructure.Persistence.Configurations;

public sealed class BingoCardConfiguration : IEntityTypeConfiguration<BingoCard>
{
	public void Configure(EntityTypeBuilder<BingoCard> builder)
	{
		builder.HasKey(item => item.Id);
		builder.HasIndex(item => item.PublicCode).IsUnique();
		builder.Property(item => item.Numbers).HasConversion(
			numbers => JsonSerializer.Serialize(Enumerable.Range(0, 5).Select(row => Enumerable.Range(0, 5).Select(column => numbers[row, column]).ToArray()).ToArray()),
			json => DeserializeNumbers(json));
	}

	private static int[,] DeserializeNumbers(string json)
	{
		var rows = JsonSerializer.Deserialize<int[][]>(json) ?? throw new InvalidOperationException("A cartela persistida é inválida.");
		var numbers = new int[5, 5];
		for (var row = 0; row < 5; row++)
		{
			for (var column = 0; column < 5; column++)
			{
				numbers[row, column] = rows[row][column];
			}
		}

		return numbers;
	}
}
