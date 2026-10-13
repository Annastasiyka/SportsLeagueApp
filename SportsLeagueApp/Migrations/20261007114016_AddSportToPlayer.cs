using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsLeagueApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSportToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SportCategory",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SportCategory",
                table: "Players");
        }
    }
}
