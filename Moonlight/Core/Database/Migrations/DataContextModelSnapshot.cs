﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Moonlight.Core.Database;

#nullable disable

namespace Moonlight.Core.Database.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Moonlight.Core.Database.Entities.ApiKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("PermissionJson")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("ApiKeys");
                });

            modelBuilder.Entity("Moonlight.Core.Database.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Flags")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Permissions")
                        .HasColumnType("int");

                    b.Property<DateTime>("TokenValidTimestamp")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("Totp")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("TotpSecret")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.Server", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Cpu")
                        .HasColumnType("int");

                    b.Property<bool>("DisablePublicNetwork")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Disk")
                        .HasColumnType("int");

                    b.Property<int>("DockerImageIndex")
                        .HasColumnType("int");

                    b.Property<int>("ImageId")
                        .HasColumnType("int");

                    b.Property<int>("MainAllocationId")
                        .HasColumnType("int");

                    b.Property<int>("Memory")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("NetworkId")
                        .HasColumnType("int");

                    b.Property<int>("NodeId")
                        .HasColumnType("int");

                    b.Property<string>("OverrideStartupCommand")
                        .HasColumnType("longtext");

                    b.Property<int>("OwnerId")
                        .HasColumnType("int");

                    b.Property<bool>("UseVirtualDisk")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("ImageId");

                    b.HasIndex("MainAllocationId");

                    b.HasIndex("NetworkId");

                    b.HasIndex("NodeId");

                    b.HasIndex("OwnerId");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerAllocation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("IpAddress")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Note")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Port")
                        .HasColumnType("int");

                    b.Property<int?>("ServerId")
                        .HasColumnType("int");

                    b.Property<int?>("ServerNodeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ServerId");

                    b.HasIndex("ServerNodeId");

                    b.ToTable("ServerAllocations");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerBackup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Completed")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int?>("ServerId")
                        .HasColumnType("int");

                    b.Property<long>("Size")
                        .HasColumnType("bigint");

                    b.Property<bool>("Successful")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("ServerId");

                    b.ToTable("ServerBackup");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerDockerImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("AutoPull")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("ServerImageId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ServerImageId");

                    b.ToTable("ServerDockerImages");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AllocationsNeeded")
                        .HasColumnType("int");

                    b.Property<bool>("AllowDockerImageChange")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("DefaultDockerImage")
                        .HasColumnType("int");

                    b.Property<string>("DonateUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("InstallDockerImage")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("InstallScript")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("InstallShell")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("OnlineDetection")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ParseConfiguration")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("StartupCommand")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("StopCommand")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("UpdateUrl")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("ServerImages");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerImageVariable", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("AllowEdit")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("AllowView")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("DefaultValue")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Filter")
                        .HasColumnType("longtext");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("ServerImageId")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ServerImageId");

                    b.ToTable("ServerImageVariables");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerNetwork", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("NodeId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("NodeId");

                    b.HasIndex("UserId");

                    b.ToTable("ServerNetworks");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerNode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Fqdn")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("FtpPort")
                        .HasColumnType("int");

                    b.Property<int>("HttpPort")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("Ssl")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("ServerNodes");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerSchedule", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ExecutionSeconds")
                        .HasColumnType("int");

                    b.Property<DateTime>("LastRun")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("ServerId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ServerId");

                    b.ToTable("ServerSchedules");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerScheduleItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("DataJson")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<int?>("ServerScheduleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ServerScheduleId");

                    b.ToTable("ServerScheduleItems");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerVariable", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("ServerId")
                        .HasColumnType("int");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("ServerId");

                    b.ToTable("ServerVariables");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.Server", b =>
                {
                    b.HasOne("Moonlight.Features.Servers.Entities.ServerImage", "Image")
                        .WithMany()
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Moonlight.Features.Servers.Entities.ServerAllocation", "MainAllocation")
                        .WithMany()
                        .HasForeignKey("MainAllocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Moonlight.Features.Servers.Entities.ServerNetwork", "Network")
                        .WithMany()
                        .HasForeignKey("NetworkId");

                    b.HasOne("Moonlight.Features.Servers.Entities.ServerNode", "Node")
                        .WithMany()
                        .HasForeignKey("NodeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Moonlight.Core.Database.Entities.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Image");

                    b.Navigation("MainAllocation");

                    b.Navigation("Network");

                    b.Navigation("Node");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerAllocation", b =>
                {
                    b.HasOne("Moonlight.Features.Servers.Entities.Server", null)
                        .WithMany("Allocations")
                        .HasForeignKey("ServerId");

                    b.HasOne("Moonlight.Features.Servers.Entities.ServerNode", null)
                        .WithMany("Allocations")
                        .HasForeignKey("ServerNodeId");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerBackup", b =>
                {
                    b.HasOne("Moonlight.Features.Servers.Entities.Server", null)
                        .WithMany("Backups")
                        .HasForeignKey("ServerId");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerDockerImage", b =>
                {
                    b.HasOne("Moonlight.Features.Servers.Entities.ServerImage", null)
                        .WithMany("DockerImages")
                        .HasForeignKey("ServerImageId");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerImageVariable", b =>
                {
                    b.HasOne("Moonlight.Features.Servers.Entities.ServerImage", null)
                        .WithMany("Variables")
                        .HasForeignKey("ServerImageId");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerNetwork", b =>
                {
                    b.HasOne("Moonlight.Features.Servers.Entities.ServerNode", "Node")
                        .WithMany()
                        .HasForeignKey("NodeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Moonlight.Core.Database.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Node");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerSchedule", b =>
                {
                    b.HasOne("Moonlight.Features.Servers.Entities.Server", null)
                        .WithMany("Schedules")
                        .HasForeignKey("ServerId");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerScheduleItem", b =>
                {
                    b.HasOne("Moonlight.Features.Servers.Entities.ServerSchedule", null)
                        .WithMany("Items")
                        .HasForeignKey("ServerScheduleId");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerVariable", b =>
                {
                    b.HasOne("Moonlight.Features.Servers.Entities.Server", null)
                        .WithMany("Variables")
                        .HasForeignKey("ServerId");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.Server", b =>
                {
                    b.Navigation("Allocations");

                    b.Navigation("Backups");

                    b.Navigation("Schedules");

                    b.Navigation("Variables");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerImage", b =>
                {
                    b.Navigation("DockerImages");

                    b.Navigation("Variables");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerNode", b =>
                {
                    b.Navigation("Allocations");
                });

            modelBuilder.Entity("Moonlight.Features.Servers.Entities.ServerSchedule", b =>
                {
                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
