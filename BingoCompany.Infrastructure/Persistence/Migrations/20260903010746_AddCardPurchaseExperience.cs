using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCardPurchaseExperience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CardPurchaseClosesAt",
                schema: "bingo",
                table: "Events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CardPurchaseLowStockThreshold",
                schema: "bingo",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CardPurchasePerParticipantLimit",
                schema: "bingo",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CardPurchaseInvitations",
                schema: "bingo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    BonusCards = table.Column<int>(type: "integer", nullable: false),
                    RedeemedByParticipantAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    RedeemedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardPurchaseInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardPurchaseInvitations_Events_EventId",
                        column: x => x.EventId,
                        principalSchema: "bingo",
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardPurchaseWaitlistEntries",
                schema: "bingo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedQuantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardPurchaseWaitlistEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardPurchaseWaitlistEntries_Events_EventId",
                        column: x => x.EventId,
                        principalSchema: "bingo",
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardPurchaseWaitlistEntries_ParticipantAccounts_Participant~",
                        column: x => x.ParticipantAccountId,
                        principalSchema: "bingo",
                        principalTable: "ParticipantAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_CardPurchaseClosesAt",
                schema: "bingo",
                table: "Events",
                column: "CardPurchaseClosesAt");

            migrationBuilder.CreateIndex(
                name: "IX_CardPurchaseInvitations_Code",
                schema: "bingo",
                table: "CardPurchaseInvitations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardPurchaseInvitations_EventId_RedeemedByParticipantAccoun~",
                schema: "bingo",
                table: "CardPurchaseInvitations",
                columns: new[] { "EventId", "RedeemedByParticipantAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_CardPurchaseWaitlistEntries_EventId_CreatedAt",
                schema: "bingo",
                table: "CardPurchaseWaitlistEntries",
                columns: new[] { "EventId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CardPurchaseWaitlistEntries_EventId_ParticipantAccountId",
                schema: "bingo",
                table: "CardPurchaseWaitlistEntries",
                columns: new[] { "EventId", "ParticipantAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardPurchaseWaitlistEntries_ParticipantAccountId",
                schema: "bingo",
                table: "CardPurchaseWaitlistEntries",
                column: "ParticipantAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardPurchaseInvitations",
                schema: "bingo");

            migrationBuilder.DropTable(
                name: "CardPurchaseWaitlistEntries",
                schema: "bingo");

            migrationBuilder.DropIndex(
                name: "IX_Events_CardPurchaseClosesAt",
                schema: "bingo",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CardPurchaseClosesAt",
                schema: "bingo",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CardPurchaseLowStockThreshold",
                schema: "bingo",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CardPurchasePerParticipantLimit",
                schema: "bingo",
                table: "Events");
        }
    }
}
