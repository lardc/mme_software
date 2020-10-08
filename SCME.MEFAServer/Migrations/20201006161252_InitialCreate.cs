using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SCME.MEFAServer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitoringEventTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringEventTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringStatTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringStatTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonitoringEventTypeId = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitoringEvents_MonitoringEventTypes_MonitoringEventTypeId",
                        column: x => x.MonitoringEventTypeId,
                        principalTable: "MonitoringEventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringStats",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonitoringStatTypeId = table.Column<int>(nullable: false),
                    MmeCode = table.Column<string>(nullable: true),
                    KeyData = table.Column<int>(nullable: false),
                    ValueData = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitoringStats_MonitoringStatTypes_MonitoringStatTypeId",
                        column: x => x.MonitoringStatTypeId,
                        principalTable: "MonitoringStatTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringEvents_MonitoringEventTypeId",
                table: "MonitoringEvents",
                column: "MonitoringEventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringStats_MonitoringStatTypeId",
                table: "MonitoringStats",
                column: "MonitoringStatTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonitoringEvents");

            migrationBuilder.DropTable(
                name: "MonitoringStats");

            migrationBuilder.DropTable(
                name: "MonitoringEventTypes");

            migrationBuilder.DropTable(
                name: "MonitoringStatTypes");
        }
    }
}
