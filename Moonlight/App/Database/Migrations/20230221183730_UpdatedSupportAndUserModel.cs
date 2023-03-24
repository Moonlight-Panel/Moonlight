using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.App.Databse.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSupportAndUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportMessages_Users_SenderId",
                table: "SupportMessages");

            migrationBuilder.AddColumn<bool>(
                name: "SupportPending",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "SenderId",
                table: "SupportMessages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsSupport",
                table: "SupportMessages",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "SupportMessages",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RecipientId",
                table: "SupportMessages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportMessages_RecipientId",
                table: "SupportMessages",
                column: "RecipientId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportMessages_Users_RecipientId",
                table: "SupportMessages",
                column: "RecipientId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportMessages_Users_SenderId",
                table: "SupportMessages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportMessages_Users_RecipientId",
                table: "SupportMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportMessages_Users_SenderId",
                table: "SupportMessages");

            migrationBuilder.DropIndex(
                name: "IX_SupportMessages_RecipientId",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "SupportPending",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsSupport",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "RecipientId",
                table: "SupportMessages");

            migrationBuilder.AlterColumn<int>(
                name: "SenderId",
                table: "SupportMessages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportMessages_Users_SenderId",
                table: "SupportMessages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
