using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class cardChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cardAssignedMember_ProjectMember_ProjectMemberUserId",
                table: "cardAssignedMember");

            migrationBuilder.RenameColumn(
                name: "ProjectMemberUserId",
                table: "cardAssignedMember",
                newName: "MemberUserId");

            migrationBuilder.RenameIndex(
                name: "IX_cardAssignedMember_ProjectMemberUserId",
                table: "cardAssignedMember",
                newName: "IX_cardAssignedMember_MemberUserId");

            migrationBuilder.AlterColumn<string>(
                name: "CardName",
                table: "Card",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Card",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_cardAssignedMember_User_MemberUserId",
                table: "cardAssignedMember",
                column: "MemberUserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cardAssignedMember_User_MemberUserId",
                table: "cardAssignedMember");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Card");

            migrationBuilder.RenameColumn(
                name: "MemberUserId",
                table: "cardAssignedMember",
                newName: "ProjectMemberUserId");

            migrationBuilder.RenameIndex(
                name: "IX_cardAssignedMember_MemberUserId",
                table: "cardAssignedMember",
                newName: "IX_cardAssignedMember_ProjectMemberUserId");

            migrationBuilder.AlterColumn<string>(
                name: "CardName",
                table: "Card",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_cardAssignedMember_ProjectMember_ProjectMemberUserId",
                table: "cardAssignedMember",
                column: "ProjectMemberUserId",
                principalTable: "ProjectMember",
                principalColumn: "ProjectMemberId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
