using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class addBaseEntityToRemTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "UserPermission",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "UserPermission",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserPermission",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "UserPermission",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "UserPermission",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "ProjectTeam",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ProjectTeam",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProjectTeam",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "ProjectTeam",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ProjectTeam",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "ProjectPermission",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ProjectPermission",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProjectPermission",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "ProjectPermission",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ProjectPermission",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "ProjectMemberPermission",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ProjectMemberPermission",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProjectMemberPermission",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "ProjectMemberPermission",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ProjectMemberPermission",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserPermission");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "UserPermission");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserPermission");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "UserPermission");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "UserPermission");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ProjectTeam");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ProjectTeam");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ProjectTeam");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ProjectTeam");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ProjectTeam");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ProjectPermission");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ProjectPermission");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ProjectPermission");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ProjectPermission");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ProjectPermission");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ProjectMemberPermission");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ProjectMemberPermission");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ProjectMemberPermission");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ProjectMemberPermission");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ProjectMemberPermission");
        }
    }
}
