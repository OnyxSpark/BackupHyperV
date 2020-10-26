using Microsoft.EntityFrameworkCore.Migrations;

namespace DB.Migrations
{
    public partial class Added_LastKnownStatus_To_History : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastKnownStatus",
                table: "History",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastKnownStatus",
                table: "History");
        }
    }
}
