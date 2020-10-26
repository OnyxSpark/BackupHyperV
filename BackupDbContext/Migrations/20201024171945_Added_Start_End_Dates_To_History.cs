using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DB.Migrations
{
    public partial class Added_Start_End_Dates_To_History : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("BackupDate", "History", "BackupDateStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "BackupDateEnd",
                table: "History",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("BackupDateStart", "History", "BackupDate");

            migrationBuilder.DropColumn(
                 name: "BackupDateEnd",
                 table: "History");
        }
    }
}
