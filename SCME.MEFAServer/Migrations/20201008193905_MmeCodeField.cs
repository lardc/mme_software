using Microsoft.EntityFrameworkCore.Migrations;

namespace SCME.MEFAServer.Migrations
{
    public partial class MmeCodeField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MmeCode",
                table: "MonitoringEvents",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MmeCode",
                table: "MonitoringEvents");
        }
    }
}
