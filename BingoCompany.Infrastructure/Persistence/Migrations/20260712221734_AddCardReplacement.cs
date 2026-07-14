using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class AddCardReplacement : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<Guid>(
				name: "ReplacementCardId",
				schema: "bingo",
				table: "Cards",
				type: "uuid",
				nullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "ReplacementCardId",
				schema: "bingo",
				table: "Cards");
		}
	}
}
