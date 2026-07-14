using BingoCompany.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BingoDbContext))]
[Migration("20260714140000_AddPrizeDeliveryState")]
public partial class AddPrizeDeliveryState : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<DateTimeOffset>(
			name: "PrizeDeclinedAt",
			schema: "bingo",
			table: "RoundWinners",
			type: "timestamp with time zone",
			nullable: true);

		migrationBuilder.AddColumn<DateTimeOffset>(
			name: "PrizeDeliveredAt",
			schema: "bingo",
			table: "RoundWinners",
			type: "timestamp with time zone",
			nullable: true);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "PrizeDeclinedAt",
			schema: "bingo",
			table: "RoundWinners");

		migrationBuilder.DropColumn(
			name: "PrizeDeliveredAt",
			schema: "bingo",
			table: "RoundWinners");
	}
}
