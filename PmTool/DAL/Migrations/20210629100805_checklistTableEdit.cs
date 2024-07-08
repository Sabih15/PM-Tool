using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class checklistTableEdit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "CheckList");

            migrationBuilder.AlterColumn<string>(
                name: "CheckListName",
                table: "CheckList",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CheckListName",
                table: "CheckList",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CheckList",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);
        }
    }
}
