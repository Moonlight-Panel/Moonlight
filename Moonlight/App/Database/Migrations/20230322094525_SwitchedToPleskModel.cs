using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.App.Database.Migrations
{
    /// <inheritdoc />
    public partial class SwitchedToPleskModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Websites_AaPanels_AaPanelId",
                table: "Websites");

            migrationBuilder.DropIndex(
                name: "IX_Websites_AaPanelId",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "DomainName",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "FtpPassword",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "FtpUsername",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "PhpVersion",
                table: "Websites");

            migrationBuilder.RenameColumn(
                name: "InternalAaPanelId",
                table: "Websites",
                newName: "PleskServerId");

            migrationBuilder.RenameColumn(
                name: "AaPanelId",
                table: "Websites",
                newName: "PleskId");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_PleskServerId",
                table: "Websites",
                column: "PleskServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Websites_PleskServers_PleskServerId",
                table: "Websites",
                column: "PleskServerId",
                principalTable: "PleskServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Websites_PleskServers_PleskServerId",
                table: "Websites");

            migrationBuilder.DropIndex(
                name: "IX_Websites_PleskServerId",
                table: "Websites");

            migrationBuilder.RenameColumn(
                name: "PleskServerId",
                table: "Websites",
                newName: "InternalAaPanelId");

            migrationBuilder.RenameColumn(
                name: "PleskId",
                table: "Websites",
                newName: "AaPanelId");

            migrationBuilder.AddColumn<string>(
                name: "DomainName",
                table: "Websites",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FtpPassword",
                table: "Websites",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FtpUsername",
                table: "Websites",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PhpVersion",
                table: "Websites",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_AaPanelId",
                table: "Websites",
                column: "AaPanelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Websites_AaPanels_AaPanelId",
                table: "Websites",
                column: "AaPanelId",
                principalTable: "AaPanels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
