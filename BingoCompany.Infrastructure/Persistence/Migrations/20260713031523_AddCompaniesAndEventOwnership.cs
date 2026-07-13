using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompaniesAndEventOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                schema: "bingo",
                table: "Events",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Companies",
                schema: "bingo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyUsers",
                schema: "bingo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyUsers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "bingo",
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("INSERT INTO bingo.\"Companies\" (\"Id\", \"Name\", \"CreatedAt\") VALUES ('11111111-1111-1111-1111-111111111111', 'Empresa legada', NOW()) ON CONFLICT (\"Id\") DO NOTHING;");
            migrationBuilder.Sql("UPDATE bingo.\"Events\" SET \"CompanyId\" = '11111111-1111-1111-1111-111111111111' WHERE \"CompanyId\" = '00000000-0000-0000-0000-000000000000';");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CompanyId",
                schema: "bingo",
                table: "Events",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_CompanyId",
                schema: "bingo",
                table: "CompanyUsers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_Email",
                schema: "bingo",
                table: "CompanyUsers",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Companies_CompanyId",
                schema: "bingo",
                table: "Events",
                column: "CompanyId",
                principalSchema: "bingo",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Companies_CompanyId",
                schema: "bingo",
                table: "Events");

            migrationBuilder.DropTable(
                name: "CompanyUsers",
                schema: "bingo");

            migrationBuilder.DropTable(
                name: "Companies",
                schema: "bingo");

            migrationBuilder.DropIndex(
                name: "IX_Events_CompanyId",
                schema: "bingo",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "bingo",
                table: "Events");
        }
    }
}
