﻿// <auto-generated />
using System;
using DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DB.Migrations
{
    [DbContext(typeof(BackupDbContext))]
    [Migration("20201026120209_Added_LastKnownStatus_To_History")]
    partial class Added_LastKnownStatus_To_History
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DB.BackupHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ArchivedToFile")
                        .HasColumnType("nvarchar(1000)")
                        .HasMaxLength(1000);

                    b.Property<DateTime?>("BackupDateEnd")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("BackupDateStart")
                        .HasColumnType("datetime2");

                    b.Property<string>("ExportedToFolder")
                        .HasColumnType("nvarchar(1000)")
                        .HasMaxLength(1000);

                    b.Property<string>("LastKnownStatus")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<bool>("Success")
                        .HasColumnType("bit");

                    b.Property<int?>("VirtualMachineId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("VirtualMachineId");

                    b.ToTable("History");
                });

            modelBuilder.Entity("DB.Hypervisor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BackupTask")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.ToTable("Hypervisors");
                });

            modelBuilder.Entity("DB.VirtualMachine", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("HypervisorId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<int?>("PercentComplete")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<DateTime?>("StatusUpdated")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("HypervisorId");

                    b.ToTable("VirtualMachines");
                });

            modelBuilder.Entity("DB.BackupHistory", b =>
                {
                    b.HasOne("DB.VirtualMachine", "VirtualMachine")
                        .WithMany("HistoryRecords")
                        .HasForeignKey("VirtualMachineId")
                        .OnDelete(DeleteBehavior.SetNull);
                });

            modelBuilder.Entity("DB.VirtualMachine", b =>
                {
                    b.HasOne("DB.Hypervisor", "Hypervisor")
                        .WithMany("VirtualMachines")
                        .HasForeignKey("HypervisorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
