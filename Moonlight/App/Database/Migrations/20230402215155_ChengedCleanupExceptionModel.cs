using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.App.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChengedCleanupExceptionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CleanupExceptions",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_CleanupExceptions_ServerId",
                table: "CleanupExceptions",
                column: "ServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CleanupExceptions_Servers_ServerId",
                table: "CleanupExceptions",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CleanupExceptions_Servers_ServerId",
                table: "CleanupExceptions");

            migrationBuilder.DropIndex(
                name: "IX_CleanupExceptions_ServerId",
                table: "CleanupExceptions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CleanupExceptions");
        }
    }
}
