using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.App.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddedServerAchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArchiveId",
                table: "Servers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Servers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Servers_ArchiveId",
                table: "Servers",
                column: "ArchiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Servers_ServerBackups_ArchiveId",
                table: "Servers",
                column: "ArchiveId",
                principalTable: "ServerBackups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servers_ServerBackups_ArchiveId",
                table: "Servers");

            migrationBuilder.DropIndex(
                name: "IX_Servers_ArchiveId",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "ArchiveId",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Servers");
        }
    }
}
