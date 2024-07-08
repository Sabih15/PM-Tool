using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class updateProjectMemberPermissionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProjectMemberPermission_MemberUserId",
                table: "ProjectMemberPermission",
                column: "MemberUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMemberPermission_ProjectId",
                table: "ProjectMemberPermission",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMemberPermission_ProjectPermissionId",
                table: "ProjectMemberPermission",
                column: "ProjectPermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMemberPermission_Project_ProjectId",
                table: "ProjectMemberPermission",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMemberPermission_ProjectPermission_ProjectPermissionId",
                table: "ProjectMemberPermission",
                column: "ProjectPermissionId",
                principalTable: "ProjectPermission",
                principalColumn: "ProjectPermissionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMemberPermission_User_MemberUserId",
                table: "ProjectMemberPermission",
                column: "MemberUserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMemberPermission_Project_ProjectId",
                table: "ProjectMemberPermission");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMemberPermission_ProjectPermission_ProjectPermissionId",
                table: "ProjectMemberPermission");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMemberPermission_User_MemberUserId",
                table: "ProjectMemberPermission");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMemberPermission_MemberUserId",
                table: "ProjectMemberPermission");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMemberPermission_ProjectId",
                table: "ProjectMemberPermission");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMemberPermission_ProjectPermissionId",
                table: "ProjectMemberPermission");
        }
    }
}
