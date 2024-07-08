using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class addPulicIdInTeammemberProjectMember : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TeamMemberPublicId",
                table: "TeamMember",
                type: "uniqueidentifier",
                nullable: true,
                defaultValueSql: "newid()");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectMemberPublicId",
                table: "ProjectMember",
                type: "uniqueidentifier",
                nullable: true,
                defaultValueSql: "newid()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamMemberPublicId",
                table: "TeamMember");

            migrationBuilder.DropColumn(
                name: "ProjectMemberPublicId",
                table: "ProjectMember");
        }
    }
}
