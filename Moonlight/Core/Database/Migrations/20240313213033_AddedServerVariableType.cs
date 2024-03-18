using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.Core.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddedServerVariableType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "ServerImageVariables",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "ServerImageVariables");
        }
    }
}
