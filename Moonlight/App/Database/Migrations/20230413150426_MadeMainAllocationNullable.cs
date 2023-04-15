using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.App.Database.Migrations
{
    /// <inheritdoc />
    public partial class MadeMainAllocationNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servers_NodeAllocations_MainAllocationId",
                table: "Servers");

            migrationBuilder.AlterColumn<int>(
                name: "MainAllocationId",
                table: "Servers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Servers_NodeAllocations_MainAllocationId",
                table: "Servers",
                column: "MainAllocationId",
                principalTable: "NodeAllocations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servers_NodeAllocations_MainAllocationId",
                table: "Servers");

            migrationBuilder.AlterColumn<int>(
                name: "MainAllocationId",
                table: "Servers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Servers_NodeAllocations_MainAllocationId",
                table: "Servers",
                column: "MainAllocationId",
                principalTable: "NodeAllocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
