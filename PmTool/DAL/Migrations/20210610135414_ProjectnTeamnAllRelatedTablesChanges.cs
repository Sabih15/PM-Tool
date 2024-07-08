using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class ProjectnTeamnAllRelatedTablesChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMember_ProjectTeam_ProjectTeamId",
                table: "ProjectMember");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMember_TemporaryProjectMember_TempProjectMemberUserId",
                table: "ProjectMember");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMember_TemporaryProjectMember_TempMemberUserId",
                table: "TeamMember");

            migrationBuilder.DropIndex(
                name: "IX_TeamMember_TempMemberUserId",
                table: "TeamMember");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMember_ProjectTeamId",
                table: "ProjectMember");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMember_TempProjectMemberUserId",
                table: "ProjectMember");

            migrationBuilder.DropColumn(
                name: "TempMemberUserId",
                table: "TeamMember");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "ProjectMember");

            migrationBuilder.DropColumn(
                name: "IsIndividualUser",
                table: "ProjectMember");

            migrationBuilder.DropColumn(
                name: "ProjectTeamId",
                table: "ProjectMember");

            migrationBuilder.DropColumn(
                name: "TempProjectMemberUserId",
                table: "ProjectMember");

            migrationBuilder.AddColumn<string>(
                name: "InviteLink",
                table: "TemporaryProjectMember",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "TemporaryProjectMember",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TemporaryTeamMember",
                columns: table => new
                {
                    TemporaryTeamMemberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemporaryTeamMemberPublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InviteLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemporaryTeamMember", x => x.TemporaryTeamMemberId);
                    table.ForeignKey(
                        name: "FK_TemporaryTeamMember_Team_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryProjectMember_ProjectId",
                table: "TemporaryProjectMember",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryTeamMember_TeamId",
                table: "TemporaryTeamMember",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_TemporaryProjectMember_Project_ProjectId",
                table: "TemporaryProjectMember",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemporaryProjectMember_Project_ProjectId",
                table: "TemporaryProjectMember");

            migrationBuilder.DropTable(
                name: "TemporaryTeamMember");

            migrationBuilder.DropIndex(
                name: "IX_TemporaryProjectMember_ProjectId",
                table: "TemporaryProjectMember");

            migrationBuilder.DropColumn(
                name: "InviteLink",
                table: "TemporaryProjectMember");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "TemporaryProjectMember");

            migrationBuilder.AddColumn<int>(
                name: "TempMemberUserId",
                table: "TeamMember",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "ProjectMember",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsIndividualUser",
                table: "ProjectMember",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectTeamId",
                table: "ProjectMember",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TempProjectMemberUserId",
                table: "ProjectMember",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_TempMemberUserId",
                table: "TeamMember",
                column: "TempMemberUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMember_ProjectTeamId",
                table: "ProjectMember",
                column: "ProjectTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMember_TempProjectMemberUserId",
                table: "ProjectMember",
                column: "TempProjectMemberUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMember_ProjectTeam_ProjectTeamId",
                table: "ProjectMember",
                column: "ProjectTeamId",
                principalTable: "ProjectTeam",
                principalColumn: "ProjectTeamId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMember_TemporaryProjectMember_TempProjectMemberUserId",
                table: "ProjectMember",
                column: "TempProjectMemberUserId",
                principalTable: "TemporaryProjectMember",
                principalColumn: "TemporaryProjectMemberId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMember_TemporaryProjectMember_TempMemberUserId",
                table: "TeamMember",
                column: "TempMemberUserId",
                principalTable: "TemporaryProjectMember",
                principalColumn: "TemporaryProjectMemberId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
