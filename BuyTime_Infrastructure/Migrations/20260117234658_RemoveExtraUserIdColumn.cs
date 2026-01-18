using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyTime_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExtraUserIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpertSocialLinks_Users_UserId",
                table: "ExpertSocialLinks");

            migrationBuilder.DropIndex(
                name: "IX_ExpertSocialLinks_UserId",
                table: "ExpertSocialLinks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExpertSocialLinks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "ExpertSocialLinks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpertSocialLinks_UserId",
                table: "ExpertSocialLinks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpertSocialLinks_Users_UserId",
                table: "ExpertSocialLinks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
