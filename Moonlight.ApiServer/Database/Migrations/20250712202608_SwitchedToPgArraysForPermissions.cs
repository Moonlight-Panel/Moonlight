using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.ApiServer.Database.Migrations
{
    /// <inheritdoc />
    public partial class SwitchedToPgArraysForPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PermissionsJson",
                table: "Core_Users");

            migrationBuilder.DropColumn(
                name: "PermissionsJson",
                table: "Core_ApiKeys");

            migrationBuilder.AddColumn<string[]>(
                name: "Permissions",
                table: "Core_Users",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "Permissions",
                table: "Core_ApiKeys",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Permissions",
                table: "Core_Users");

            migrationBuilder.DropColumn(
                name: "Permissions",
                table: "Core_ApiKeys");

            migrationBuilder.AddColumn<string>(
                name: "PermissionsJson",
                table: "Core_Users",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PermissionsJson",
                table: "Core_ApiKeys",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
