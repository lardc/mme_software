using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SCME.MEFADB.Migrations
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
                    MmeCode = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Data1 = table.Column<long>(nullable: false),
                    Data2 = table.Column<long>(nullable: false),
                    Data3 = table.Column<long>(nullable: false),
                    Data4 = table.Column<string>(nullable: true)
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

            migrationBuilder.InsertData(
                table: "MonitoringEventTypes",
                columns: new[] { "Id", "EventName" },
                values: new object[,]
                {
                    { 1, "MME_ERROR" },
                    { 2, "MME_START" },
                    { 3, "MME_SYNC" },
                    { 4, "MME_TEST" },
                    { 5, "MME_HEART_BEAT" }
                });

            migrationBuilder.InsertData(
                table: "MonitoringStatTypes",
                columns: new[] { "Id", "StatName" },
                values: new object[,]
                {
                    { 1, "DAY_HOURS" },
                    { 2, "TOTAL_HOURS" },
                    { 3, "LAST_START_HOURS" }
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
