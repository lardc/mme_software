using Microsoft.EntityFrameworkCore.Migrations;

namespace SCME.MEFAServer.Migrations
{
    public partial class Data1234 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Data1",
                table: "MonitoringEvents",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Data2",
                table: "MonitoringEvents",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Data3",
                table: "MonitoringEvents",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Data4",
                table: "MonitoringEvents",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data1",
                table: "MonitoringEvents");

            migrationBuilder.DropColumn(
                name: "Data2",
                table: "MonitoringEvents");

            migrationBuilder.DropColumn(
                name: "Data3",
                table: "MonitoringEvents");

            migrationBuilder.DropColumn(
                name: "Data4",
                table: "MonitoringEvents");
        }
    }
}
