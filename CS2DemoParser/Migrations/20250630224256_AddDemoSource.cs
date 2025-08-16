using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS2DemoParser.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DemoSource",
                table: "DemoFiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DemoSource",
                table: "DemoFiles");
        }
    }
}
