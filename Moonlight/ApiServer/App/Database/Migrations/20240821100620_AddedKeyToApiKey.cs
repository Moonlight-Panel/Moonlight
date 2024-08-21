using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.ApiServer.App.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddedKeyToApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Key",
                schema: "Core",
                table: "ApiKeys",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                schema: "Core",
                table: "ApiKeys");
        }
    }
}
