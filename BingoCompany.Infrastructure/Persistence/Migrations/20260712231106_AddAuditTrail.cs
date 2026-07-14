using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class AddAuditTrail : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "AuditEntries",
				schema: "bingo",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					EventId = table.Column<Guid>(type: "uuid", nullable: false),
					Action = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
					Details = table.Column<string>(type: "text", nullable: false),
					OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AuditEntries", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_AuditEntries_EventId_OccurredAt",
				schema: "bingo",
				table: "AuditEntries",
				columns: new[] { "EventId", "OccurredAt" });
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "AuditEntries",
				schema: "bingo");
		}
	}
}
