using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class InitialPostgreSql : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.EnsureSchema(
				name: "bingo");

			migrationBuilder.CreateTable(
				name: "CardMarks",
				schema: "bingo",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					RoundId = table.Column<Guid>(type: "uuid", nullable: false),
					CardId = table.Column<Guid>(type: "uuid", nullable: false),
					Number = table.Column<int>(type: "integer", nullable: false),
					DrawSequence = table.Column<int>(type: "integer", nullable: false),
					MarkedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_CardMarks", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Events",
				schema: "bingo",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					Name = table.Column<string>(type: "text", nullable: false),
					PublicCode = table.Column<string>(type: "text", nullable: false),
					Status = table.Column<int>(type: "integer", nullable: false),
					CardsPerParticipant = table.Column<int>(type: "integer", nullable: false),
					MarkingMode = table.Column<int>(type: "integer", nullable: false),
					CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Events", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "RoundWinners",
				schema: "bingo",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					RoundId = table.Column<Guid>(type: "uuid", nullable: false),
					StageId = table.Column<Guid>(type: "uuid", nullable: false),
					CardId = table.Column<Guid>(type: "uuid", nullable: false),
					ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
					DrawSequence = table.Column<int>(type: "integer", nullable: false),
					TieBreakerNumber = table.Column<int>(type: "integer", nullable: true),
					IsWinner = table.Column<bool>(type: "boolean", nullable: false),
					DetectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
					RevealedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_RoundWinners", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Cards",
				schema: "bingo",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					EventId = table.Column<Guid>(type: "uuid", nullable: false),
					ParticipantId = table.Column<Guid>(type: "uuid", nullable: true),
					Type = table.Column<int>(type: "integer", nullable: false),
					Status = table.Column<int>(type: "integer", nullable: false),
					PublicCode = table.Column<string>(type: "text", nullable: false),
					Numbers = table.Column<string>(type: "text", nullable: false),
					CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Cards", x => x.Id);
					table.ForeignKey(
						name: "FK_Cards_Events_EventId",
						column: x => x.EventId,
						principalSchema: "bingo",
						principalTable: "Events",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Participants",
				schema: "bingo",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					EventId = table.Column<Guid>(type: "uuid", nullable: false),
					Name = table.Column<string>(type: "text", nullable: false),
					ResponsibleEmployeeName = table.Column<string>(type: "text", nullable: true),
					JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Participants", x => x.Id);
					table.ForeignKey(
						name: "FK_Participants_Events_EventId",
						column: x => x.EventId,
						principalSchema: "bingo",
						principalTable: "Events",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Rounds",
				schema: "bingo",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					EventId = table.Column<Guid>(type: "uuid", nullable: false),
					Sequence = table.Column<int>(type: "integer", nullable: false),
					Name = table.Column<string>(type: "text", nullable: false),
					Status = table.Column<int>(type: "integer", nullable: false),
					SequenceHash = table.Column<string>(type: "text", nullable: true),
					DrawSequence = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Rounds", x => x.Id);
					table.ForeignKey(
						name: "FK_Rounds_Events_EventId",
						column: x => x.EventId,
						principalSchema: "bingo",
						principalTable: "Events",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "DrawnNumbers",
				schema: "bingo",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					RoundId = table.Column<Guid>(type: "uuid", nullable: false),
					Number = table.Column<int>(type: "integer", nullable: false),
					Sequence = table.Column<int>(type: "integer", nullable: false),
					DrawnAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_DrawnNumbers", x => x.Id);
					table.ForeignKey(
						name: "FK_DrawnNumbers_Rounds_RoundId",
						column: x => x.RoundId,
						principalSchema: "bingo",
						principalTable: "Rounds",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "PrizeStages",
				schema: "bingo",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					RoundId = table.Column<Guid>(type: "uuid", nullable: false),
					Sequence = table.Column<int>(type: "integer", nullable: false),
					PrizeName = table.Column<string>(type: "text", nullable: false),
					Pattern = table.Column<int>(type: "integer", nullable: false),
					IsActive = table.Column<bool>(type: "boolean", nullable: false),
					IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PrizeStages", x => x.Id);
					table.ForeignKey(
						name: "FK_PrizeStages_Rounds_RoundId",
						column: x => x.RoundId,
						principalSchema: "bingo",
						principalTable: "Rounds",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "RoundEligibleCards",
				schema: "bingo",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					RoundId = table.Column<Guid>(type: "uuid", nullable: false),
					CardId = table.Column<Guid>(type: "uuid", nullable: false),
					ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
					IncludedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_RoundEligibleCards", x => x.Id);
					table.ForeignKey(
						name: "FK_RoundEligibleCards_Rounds_RoundId",
						column: x => x.RoundId,
						principalSchema: "bingo",
						principalTable: "Rounds",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_CardMarks_RoundId_CardId_Number",
				schema: "bingo",
				table: "CardMarks",
				columns: new[] { "RoundId", "CardId", "Number" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Cards_EventId",
				schema: "bingo",
				table: "Cards",
				column: "EventId");

			migrationBuilder.CreateIndex(
				name: "IX_Cards_PublicCode",
				schema: "bingo",
				table: "Cards",
				column: "PublicCode",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_DrawnNumbers_RoundId",
				schema: "bingo",
				table: "DrawnNumbers",
				column: "RoundId");

			migrationBuilder.CreateIndex(
				name: "IX_Events_PublicCode",
				schema: "bingo",
				table: "Events",
				column: "PublicCode",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Participants_EventId",
				schema: "bingo",
				table: "Participants",
				column: "EventId");

			migrationBuilder.CreateIndex(
				name: "IX_PrizeStages_RoundId",
				schema: "bingo",
				table: "PrizeStages",
				column: "RoundId");

			migrationBuilder.CreateIndex(
				name: "IX_RoundEligibleCards_RoundId_CardId",
				schema: "bingo",
				table: "RoundEligibleCards",
				columns: new[] { "RoundId", "CardId" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Rounds_EventId",
				schema: "bingo",
				table: "Rounds",
				column: "EventId");

			migrationBuilder.CreateIndex(
				name: "IX_RoundWinners_StageId_CardId",
				schema: "bingo",
				table: "RoundWinners",
				columns: new[] { "StageId", "CardId" },
				unique: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "CardMarks",
				schema: "bingo");

			migrationBuilder.DropTable(
				name: "Cards",
				schema: "bingo");

			migrationBuilder.DropTable(
				name: "DrawnNumbers",
				schema: "bingo");

			migrationBuilder.DropTable(
				name: "Participants",
				schema: "bingo");

			migrationBuilder.DropTable(
				name: "PrizeStages",
				schema: "bingo");

			migrationBuilder.DropTable(
				name: "RoundEligibleCards",
				schema: "bingo");

			migrationBuilder.DropTable(
				name: "RoundWinners",
				schema: "bingo");

			migrationBuilder.DropTable(
				name: "Rounds",
				schema: "bingo");

			migrationBuilder.DropTable(
				name: "Events",
				schema: "bingo");
		}
	}
}
