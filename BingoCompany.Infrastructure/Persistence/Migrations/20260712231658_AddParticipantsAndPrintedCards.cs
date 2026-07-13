using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipantsAndPrintedCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeeRegistration",
                schema: "bingo",
                table: "Participants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "bingo",
                table: "Participants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Fingerprint",
                schema: "bingo",
                table: "Cards",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeRegistration",
                schema: "bingo",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "bingo",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "Fingerprint",
                schema: "bingo",
                table: "Cards");
        }
    }
}
