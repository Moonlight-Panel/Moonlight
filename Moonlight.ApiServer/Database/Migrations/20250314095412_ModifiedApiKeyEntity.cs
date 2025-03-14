using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.ApiServer.Database.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedApiKeyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Secret",
                table: "Core_ApiKeys");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Core_ApiKeys",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Core_ApiKeys");

            migrationBuilder.AddColumn<string>(
                name: "Secret",
                table: "Core_ApiKeys",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
