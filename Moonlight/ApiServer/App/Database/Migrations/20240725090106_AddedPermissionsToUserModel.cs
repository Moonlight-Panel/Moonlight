using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.ApiServer.App.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddedPermissionsToUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PermissionsJson",
                schema: "Core",
                table: "Users",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PermissionsJson",
                schema: "Core",
                table: "Users");
        }
    }
}
