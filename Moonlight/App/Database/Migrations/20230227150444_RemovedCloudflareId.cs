using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.App.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemovedCloudflareId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloudflareId",
                table: "Domains");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CloudflareId",
                table: "Domains",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
