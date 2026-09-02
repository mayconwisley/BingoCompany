using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoundDrawConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DrawnNumbers_RoundId",
                schema: "bingo",
                table: "DrawnNumbers");

            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "bingo",
                table: "Rounds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DrawnNumbers_RoundId_Number",
                schema: "bingo",
                table: "DrawnNumbers",
                columns: new[] { "RoundId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrawnNumbers_RoundId_Sequence",
                schema: "bingo",
                table: "DrawnNumbers",
                columns: new[] { "RoundId", "Sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DrawnNumbers_RoundId_Number",
                schema: "bingo",
                table: "DrawnNumbers");

            migrationBuilder.DropIndex(
                name: "IX_DrawnNumbers_RoundId_Sequence",
                schema: "bingo",
                table: "DrawnNumbers");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "bingo",
                table: "Rounds");

            migrationBuilder.CreateIndex(
                name: "IX_DrawnNumbers_RoundId",
                schema: "bingo",
                table: "DrawnNumbers",
                column: "RoundId");
        }
    }
}
