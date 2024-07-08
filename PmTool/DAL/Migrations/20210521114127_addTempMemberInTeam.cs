using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class addTempMemberInTeam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "TemporaryProjectMember");

            migrationBuilder.AddColumn<int>(
                name: "TempMemberUserId",
                table: "TeamMember",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_TempMemberUserId",
                table: "TeamMember",
                column: "TempMemberUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMember_TemporaryProjectMember_TempMemberUserId",
                table: "TeamMember",
                column: "TempMemberUserId",
                principalTable: "TemporaryProjectMember",
                principalColumn: "TemporaryProjectMemberId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamMember_TemporaryProjectMember_TempMemberUserId",
                table: "TeamMember");

            migrationBuilder.DropIndex(
                name: "IX_TeamMember_TempMemberUserId",
                table: "TeamMember");

            migrationBuilder.DropColumn(
                name: "TempMemberUserId",
                table: "TeamMember");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "TemporaryProjectMember",
                type: "int",
                nullable: true);
        }
    }
}
