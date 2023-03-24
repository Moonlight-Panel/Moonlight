using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.App.Database.Migrations
{
    /// <inheritdoc />
    public partial class SwitchedToJsonTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageTags_Images_ImageId",
                table: "ImageTags");

            migrationBuilder.DropIndex(
                name: "IX_ImageTags_ImageId",
                table: "ImageTags");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "ImageTags");

            migrationBuilder.AddColumn<string>(
                name: "TagsJson",
                table: "Images",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "InternalAaPanelId",
                table: "Databases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Databases",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AaPanels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Url = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Key = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BaseDomain = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AaPanels", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Websites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InternalAaPanelId = table.Column<int>(type: "int", nullable: false),
                    AaPanelId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    DomainName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhpVersion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FtpUsername = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FtpPassword = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Websites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Websites_AaPanels_AaPanelId",
                        column: x => x.AaPanelId,
                        principalTable: "AaPanels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Websites_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Databases_AaPanelId",
                table: "Databases",
                column: "AaPanelId");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_AaPanelId",
                table: "Websites",
                column: "AaPanelId");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_OwnerId",
                table: "Websites",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Databases_AaPanels_AaPanelId",
                table: "Databases",
                column: "AaPanelId",
                principalTable: "AaPanels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Databases_AaPanels_AaPanelId",
                table: "Databases");

            migrationBuilder.DropTable(
                name: "Websites");

            migrationBuilder.DropTable(
                name: "AaPanels");

            migrationBuilder.DropIndex(
                name: "IX_Databases_AaPanelId",
                table: "Databases");

            migrationBuilder.DropColumn(
                name: "TagsJson",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "InternalAaPanelId",
                table: "Databases");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Databases");

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "ImageTags",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageTags_ImageId",
                table: "ImageTags",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageTags_Images_ImageId",
                table: "ImageTags",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id");
        }
    }
}
