using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using BingoCompany.Infrastructure.Persistence;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BingoDbContext))]
[Migration("20260713010000_AddPrizeImages")]
public partial class AddPrizeImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "PrizeImageDataUrl", schema: "bingo", table: "PrizeStages", type: "character varying(2800000)", maxLength: 2800000, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PrizeImageDataUrl", schema: "bingo", table: "PrizeStages");
    }
}
