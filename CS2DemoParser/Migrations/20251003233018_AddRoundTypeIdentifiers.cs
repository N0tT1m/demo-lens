using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS2DemoParser.Migrations
{
    /// <inheritdoc />
    public partial class AddRoundTypeIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GamePhase",
                table: "Rounds",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsKnifeRound",
                table: "Rounds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWarmup",
                table: "Rounds",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GamePhase",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "IsKnifeRound",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "IsWarmup",
                table: "Rounds");
        }
    }
}
