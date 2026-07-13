using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BingoCompany.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDiagonalWinningPatterns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("UPDATE bingo.\"PrizeStages\" SET \"Pattern\" = 3 WHERE \"Pattern\" IN (4, 5);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
