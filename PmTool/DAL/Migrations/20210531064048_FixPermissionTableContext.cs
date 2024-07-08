using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class FixPermissionTableContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPermission_projectpermissionid_PermissionId",
                table: "UserPermission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_projectpermissionid",
                table: "projectpermissionid");

            migrationBuilder.RenameTable(
                name: "projectpermissionid",
                newName: "Permission");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Permission",
                table: "Permission",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermission_Permission_PermissionId",
                table: "UserPermission",
                column: "PermissionId",
                principalTable: "Permission",
                principalColumn: "PermissionId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPermission_Permission_PermissionId",
                table: "UserPermission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Permission",
                table: "Permission");

            migrationBuilder.RenameTable(
                name: "Permission",
                newName: "projectpermissionid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_projectpermissionid",
                table: "projectpermissionid",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermission_projectpermissionid_PermissionId",
                table: "UserPermission",
                column: "PermissionId",
                principalTable: "projectpermissionid",
                principalColumn: "PermissionId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
