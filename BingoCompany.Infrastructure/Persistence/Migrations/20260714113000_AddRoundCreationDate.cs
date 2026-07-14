using BingoCompany.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BingoDbContext))]
[Migration("20260714113000_AddRoundCreationDate")]
public partial class AddRoundCreationDate : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<DateTimeOffset>(
			name: "CreatedAt",
			schema: "bingo",
			table: "Rounds",
			type: "timestamp with time zone",
			nullable: false,
			defaultValueSql: "CURRENT_TIMESTAMP");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "CreatedAt",
			schema: "bingo",
			table: "Rounds");
	}
}
