using BingoCompany.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BingoDbContext))]
[Migration("20260715113000_AddCardPurchaseCancellationReason")]
public partial class AddCardPurchaseCancellationReason : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(name: "CardPurchaseCancellationReason", schema: "bingo", table: "Events", type: "character varying(500)", maxLength: 500, nullable: true);
		migrationBuilder.AddColumn<string>(name: "InvalidationReason", schema: "bingo", table: "Cards", type: "character varying(500)", maxLength: 500, nullable: true);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(name: "CardPurchaseCancellationReason", schema: "bingo", table: "Events");
		migrationBuilder.DropColumn(name: "InvalidationReason", schema: "bingo", table: "Cards");
	}
}
