using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.Core.Database.Migrations
{
    /// <inheritdoc />
    public partial class OldMigrationsAsASingleForMysql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServerImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Author = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdateUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DonateUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartupCommand = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OnlineDetection = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StopCommand = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstallShell = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstallDockerImage = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstallScript = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParseConfiguration = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AllocationsNeeded = table.Column<int>(type: "int", nullable: false),
                    DefaultDockerImage = table.Column<int>(type: "int", nullable: false),
                    AllowDockerImageChange = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerImages", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServerNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fqdn = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HttpPort = table.Column<int>(type: "int", nullable: false),
                    FtpPort = table.Column<int>(type: "int", nullable: false),
                    Ssl = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Token = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerNodes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TokenValidTimestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Totp = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TotpSecret = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Permissions = table.Column<int>(type: "int", nullable: false),
                    Flags = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServerDockerImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DisplayName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AutoPull = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ServerImageId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerDockerImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerDockerImages_ServerImages_ServerImageId",
                        column: x => x.ServerImageId,
                        principalTable: "ServerImages",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServerImageVariables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultValue = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AllowView = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowEdit = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Filter = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServerImageId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerImageVariables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerImageVariables_ServerImages_ServerImageId",
                        column: x => x.ServerImageId,
                        principalTable: "ServerImages",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServerNetworks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    NodeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerNetworks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerNetworks_ServerNodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "ServerNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServerNetworks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServerAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IpAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServerId = table.Column<int>(type: "int", nullable: true),
                    ServerNodeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerAllocations_ServerNodes_ServerNodeId",
                        column: x => x.ServerNodeId,
                        principalTable: "ServerNodes",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    ImageId = table.Column<int>(type: "int", nullable: false),
                    DockerImageIndex = table.Column<int>(type: "int", nullable: false),
                    OverrideStartupCommand = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cpu = table.Column<int>(type: "int", nullable: false),
                    Memory = table.Column<int>(type: "int", nullable: false),
                    Disk = table.Column<int>(type: "int", nullable: false),
                    UseVirtualDisk = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NodeId = table.Column<int>(type: "int", nullable: false),
                    NetworkId = table.Column<int>(type: "int", nullable: true),
                    DisablePublicNetwork = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MainAllocationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Servers_ServerAllocations_MainAllocationId",
                        column: x => x.MainAllocationId,
                        principalTable: "ServerAllocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_ServerImages_ImageId",
                        column: x => x.ImageId,
                        principalTable: "ServerImages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_ServerNetworks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "ServerNetworks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Servers_ServerNodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "ServerNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServerBackup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Successful = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Completed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ServerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerBackup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerBackup_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServerSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastRun = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExecutionSeconds = table.Column<int>(type: "int", nullable: false),
                    ServerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerSchedules_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServerVariables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerVariables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerVariables_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServerScheduleItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ServerScheduleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerScheduleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerScheduleItems_ServerSchedules_ServerScheduleId",
                        column: x => x.ServerScheduleId,
                        principalTable: "ServerSchedules",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ServerAllocations_ServerId",
                table: "ServerAllocations",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerAllocations_ServerNodeId",
                table: "ServerAllocations",
                column: "ServerNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerBackup_ServerId",
                table: "ServerBackup",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerDockerImages_ServerImageId",
                table: "ServerDockerImages",
                column: "ServerImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerImageVariables_ServerImageId",
                table: "ServerImageVariables",
                column: "ServerImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerNetworks_NodeId",
                table: "ServerNetworks",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerNetworks_UserId",
                table: "ServerNetworks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_ImageId",
                table: "Servers",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_MainAllocationId",
                table: "Servers",
                column: "MainAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_NetworkId",
                table: "Servers",
                column: "NetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_NodeId",
                table: "Servers",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_OwnerId",
                table: "Servers",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerScheduleItems_ServerScheduleId",
                table: "ServerScheduleItems",
                column: "ServerScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerSchedules_ServerId",
                table: "ServerSchedules",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerVariables_ServerId",
                table: "ServerVariables",
                column: "ServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerAllocations_Servers_ServerId",
                table: "ServerAllocations",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerAllocations_ServerNodes_ServerNodeId",
                table: "ServerAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerNetworks_ServerNodes_NodeId",
                table: "ServerNetworks");

            migrationBuilder.DropForeignKey(
                name: "FK_Servers_ServerNodes_NodeId",
                table: "Servers");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerAllocations_Servers_ServerId",
                table: "ServerAllocations");

            migrationBuilder.DropTable(
                name: "ServerBackup");

            migrationBuilder.DropTable(
                name: "ServerDockerImages");

            migrationBuilder.DropTable(
                name: "ServerImageVariables");

            migrationBuilder.DropTable(
                name: "ServerScheduleItems");

            migrationBuilder.DropTable(
                name: "ServerVariables");

            migrationBuilder.DropTable(
                name: "ServerSchedules");

            migrationBuilder.DropTable(
                name: "ServerNodes");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropTable(
                name: "ServerAllocations");

            migrationBuilder.DropTable(
                name: "ServerImages");

            migrationBuilder.DropTable(
                name: "ServerNetworks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
