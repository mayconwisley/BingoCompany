using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditEntryRoundReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoundId",
                schema: "bingo",
                table: "AuditEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_EventId_RoundId_OccurredAt",
                schema: "bingo",
                table: "AuditEntries",
                columns: new[] { "EventId", "RoundId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditEntries_EventId_RoundId_OccurredAt",
                schema: "bingo",
                table: "AuditEntries");

            migrationBuilder.DropColumn(
                name: "RoundId",
                schema: "bingo",
                table: "AuditEntries");
        }
    }
}
