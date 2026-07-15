using BingoCompany.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BingoDbContext))]
[Migration("20260715103000_AddParticipantAccounts")]
public partial class AddParticipantAccounts : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "ParticipantAccounts",
			schema: "bingo",
			columns: table => new
			{
				Id = table.Column<Guid>(type: "uuid", nullable: false),
				Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
				Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
				PasswordHash = table.Column<string>(type: "text", nullable: false),
				CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table => table.PrimaryKey("PK_ParticipantAccounts", x => x.Id));
		migrationBuilder.CreateIndex(name: "IX_ParticipantAccounts_Email", schema: "bingo", table: "ParticipantAccounts", column: "Email", unique: true);
		migrationBuilder.AddColumn<Guid>(name: "ParticipantAccountId", schema: "bingo", table: "Participants", type: "uuid", nullable: true);
		migrationBuilder.CreateIndex(name: "IX_Participants_ParticipantAccountId", schema: "bingo", table: "Participants", column: "ParticipantAccountId");
		migrationBuilder.AddForeignKey(name: "FK_Participants_ParticipantAccounts_ParticipantAccountId", schema: "bingo", table: "Participants", column: "ParticipantAccountId", principalSchema: "bingo", principalTable: "ParticipantAccounts", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(name: "FK_Participants_ParticipantAccounts_ParticipantAccountId", schema: "bingo", table: "Participants");
		migrationBuilder.DropIndex(name: "IX_Participants_ParticipantAccountId", schema: "bingo", table: "Participants");
		migrationBuilder.DropColumn(name: "ParticipantAccountId", schema: "bingo", table: "Participants");
		migrationBuilder.DropTable(name: "ParticipantAccounts", schema: "bingo");
	}
}
