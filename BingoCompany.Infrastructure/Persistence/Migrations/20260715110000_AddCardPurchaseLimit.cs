using BingoCompany.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BingoDbContext))]
[Migration("20260715110000_AddCardPurchaseLimit")]
public partial class AddCardPurchaseLimit : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<int>(name: "CardPurchaseLimit", schema: "bingo", table: "Events", type: "integer", nullable: true);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(name: "CardPurchaseLimit", schema: "bingo", table: "Events");
	}
}
