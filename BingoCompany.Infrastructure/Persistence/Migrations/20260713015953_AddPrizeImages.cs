using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPrizeImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrizeImageDataUrl",
                schema: "bingo",
                table: "PrizeStages",
                type: "character varying(2800000)",
                maxLength: 2800000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrizeImageDataUrl",
                schema: "bingo",
                table: "PrizeStages");
        }
    }
}
