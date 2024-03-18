using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.Core.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixedMissingPropertyInServerImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServerImageId",
                table: "ServerImageVariables",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServerImageVariables_ServerImageId",
                table: "ServerImageVariables",
                column: "ServerImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerImageVariables_ServerImages_ServerImageId",
                table: "ServerImageVariables",
                column: "ServerImageId",
                principalTable: "ServerImages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerImageVariables_ServerImages_ServerImageId",
                table: "ServerImageVariables");

            migrationBuilder.DropIndex(
                name: "IX_ServerImageVariables_ServerImageId",
                table: "ServerImageVariables");

            migrationBuilder.DropColumn(
                name: "ServerImageId",
                table: "ServerImageVariables");
        }
    }
}
