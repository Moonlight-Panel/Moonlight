using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonlight.Core.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddedServerModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServerImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AllocationsNeeded = table.Column<int>(type: "INTEGER", nullable: false),
                    StartupCommand = table.Column<string>(type: "TEXT", nullable: false),
                    StopCommand = table.Column<string>(type: "TEXT", nullable: false),
                    OnlineDetection = table.Column<string>(type: "TEXT", nullable: false),
                    ParseConfigurations = table.Column<string>(type: "TEXT", nullable: false),
                    InstallDockerImage = table.Column<string>(type: "TEXT", nullable: false),
                    InstallShell = table.Column<string>(type: "TEXT", nullable: false),
                    InstallScript = table.Column<string>(type: "TEXT", nullable: false),
                    Author = table.Column<string>(type: "TEXT", nullable: false),
                    DonateUrl = table.Column<string>(type: "TEXT", nullable: true),
                    UpdateUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DefaultDockerImageIndex = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerImages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerImageVariables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultValue = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    AllowUserToEdit = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowUserToView = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerImageVariables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Fqdn = table.Column<string>(type: "TEXT", nullable: false),
                    UseSsl = table.Column<bool>(type: "INTEGER", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    HttpPort = table.Column<int>(type: "INTEGER", nullable: false),
                    FtpPort = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerNodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerDockerImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    AutoPull = table.Column<bool>(type: "INTEGER", nullable: false),
                    ServerImageId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerDockerImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerDockerImages_ServerImages_ServerImageId",
                        column: x => x.ServerImageId,
                        principalTable: "ServerImages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServerAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    ServerId = table.Column<int>(type: "INTEGER", nullable: true),
                    ServerNodeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerAllocations_ServerNodes_ServerNodeId",
                        column: x => x.ServerNodeId,
                        principalTable: "ServerNodes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cpu = table.Column<int>(type: "INTEGER", nullable: false),
                    Memory = table.Column<int>(type: "INTEGER", nullable: false),
                    Disk = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageId = table.Column<int>(type: "INTEGER", nullable: false),
                    DockerImageIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    OverrideStartupCommand = table.Column<string>(type: "TEXT", nullable: true),
                    NodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    MainAllocationId = table.Column<int>(type: "INTEGER", nullable: false)
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
                        name: "FK_Servers_ServerNodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "ServerNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServerVariables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    ServerId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerVariables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerVariables_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServerAllocations_ServerId",
                table: "ServerAllocations",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerAllocations_ServerNodeId",
                table: "ServerAllocations",
                column: "ServerNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerDockerImages_ServerImageId",
                table: "ServerDockerImages",
                column: "ServerImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_ImageId",
                table: "Servers",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_MainAllocationId",
                table: "Servers",
                column: "MainAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_NodeId",
                table: "Servers",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_ServiceId",
                table: "Servers",
                column: "ServiceId");

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
                name: "FK_Servers_ServerNodes_NodeId",
                table: "Servers");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerAllocations_Servers_ServerId",
                table: "ServerAllocations");

            migrationBuilder.DropTable(
                name: "ServerDockerImages");

            migrationBuilder.DropTable(
                name: "ServerImageVariables");

            migrationBuilder.DropTable(
                name: "ServerVariables");

            migrationBuilder.DropTable(
                name: "ServerNodes");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropTable(
                name: "ServerAllocations");

            migrationBuilder.DropTable(
                name: "ServerImages");
        }
    }
}
