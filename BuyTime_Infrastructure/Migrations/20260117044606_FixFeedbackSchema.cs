using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyTime_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixFeedbackSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Users_UserId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Users_UserId1",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_UserId1",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Feedbacks");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Feedbacks",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_UserId",
                table: "Feedbacks",
                newName: "IX_Feedbacks_StudentId");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Feedbacks",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExpertId",
                table: "Feedbacks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ExpertId",
                table: "Feedbacks",
                column: "ExpertId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Users_ExpertId",
                table: "Feedbacks",
                column: "ExpertId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Users_StudentId",
                table: "Feedbacks",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Users_ExpertId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Users_StudentId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_ExpertId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "ExpertId",
                table: "Feedbacks");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Feedbacks",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_StudentId",
                table: "Feedbacks",
                newName: "IX_Feedbacks_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Feedbacks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Feedbacks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_UserId1",
                table: "Feedbacks",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Users_UserId",
                table: "Feedbacks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Users_UserId1",
                table: "Feedbacks",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
