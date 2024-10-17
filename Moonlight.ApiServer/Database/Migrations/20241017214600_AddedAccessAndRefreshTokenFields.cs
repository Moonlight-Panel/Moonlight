using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.ApiServer.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddedAccessAndRefreshTokenFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                schema: "Core",
                table: "Users",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTimestamp",
                schema: "Core",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
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
                name: "AccessToken",
                schema: "Core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTimestamp",
                schema: "Core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                schema: "Core",
                table: "Users");
        }
    }
}
