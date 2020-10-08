﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SCME.MEFAServer;

namespace SCME.MEFAServer.Migrations
{
    [DbContext(typeof(MonitoringContext))]
    [Migration("20201006161252_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SCME.MEFAServer.Tables.MonitoringEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("MonitoringEventTypeId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("MonitoringEventTypeId");

                    b.ToTable("MonitoringEvents");
                });

            modelBuilder.Entity("SCME.MEFAServer.Tables.MonitoringEventType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("EventName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("MonitoringEventTypes");
                });

            modelBuilder.Entity("SCME.MEFAServer.Tables.MonitoringStat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("KeyData")
                        .HasColumnType("int");

                    b.Property<string>("MmeCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MonitoringStatTypeId")
                        .HasColumnType("int");

                    b.Property<string>("ValueData")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("MonitoringStatTypeId");

                    b.ToTable("MonitoringStats");
                });

            modelBuilder.Entity("SCME.MEFAServer.Tables.MonitoringStatType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("StatName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("MonitoringStatTypes");
                });

            modelBuilder.Entity("SCME.MEFAServer.Tables.MonitoringEvent", b =>
                {
                    b.HasOne("SCME.MEFAServer.Tables.MonitoringEventType", "MonitoringEventType")
                        .WithMany()
                        .HasForeignKey("MonitoringEventTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SCME.MEFAServer.Tables.MonitoringStat", b =>
                {
                    b.HasOne("SCME.MEFAServer.Tables.MonitoringStatType", "MonitoringStatType")
                        .WithMany()
                        .HasForeignKey("MonitoringStatTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
