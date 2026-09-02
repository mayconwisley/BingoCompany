using BingoCompany.Domain.Entities;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class RoundDrawPersistenceConfigurationTests
{
	[Fact]
	public void Configures_concurrency_and_unique_constraints_for_drawn_numbers()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.CreateVersion7().ToString()).Options;
		using var db = new BingoDbContext(options);
		var round = db.Model.FindEntityType(typeof(BingoRound))!;
		var drawnNumber = db.Model.FindEntityType(typeof(DrawnNumber))!;

		Assert.True(round.FindProperty(nameof(BingoRound.Version))!.IsConcurrencyToken);
		Assert.Contains(drawnNumber.GetIndexes(), index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual([nameof(DrawnNumber.RoundId), nameof(DrawnNumber.Number)]));
		Assert.Contains(drawnNumber.GetIndexes(), index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual([nameof(DrawnNumber.RoundId), nameof(DrawnNumber.Sequence)]));
	}
}
