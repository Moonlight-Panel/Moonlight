using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.Core.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddedServerSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServerSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Cron = table.Column<string>(type: "TEXT", nullable: false),
                    ActionType = table.Column<int>(type: "INTEGER", nullable: false),
                    ActionData = table.Column<string>(type: "TEXT", nullable: false),
                    LastRunAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WasLastRunAutomatic = table.Column<bool>(type: "INTEGER", nullable: false),
                    ServerId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerSchedules_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServerSchedules_ServerId",
                table: "ServerSchedules",
                column: "ServerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerSchedules");
        }
    }
}
