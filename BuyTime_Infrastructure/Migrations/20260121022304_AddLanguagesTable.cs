using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyTime_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LanguageSkill_UserId",
                table: "LanguageSkill");

            migrationBuilder.DropColumn(
                name: "LanguageName",
                table: "LanguageSkill");

            migrationBuilder.AddColumn<Guid>(
                name: "LanguageId",
                table: "LanguageSkill",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LanguageSkill_LanguageId",
                table: "LanguageSkill",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageSkill_UserId_LanguageId",
                table: "LanguageSkill",
                columns: new[] { "UserId", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Languages_Name",
                table: "Languages",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LanguageSkill_Languages_LanguageId",
                table: "LanguageSkill",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LanguageSkill_Languages_LanguageId",
                table: "LanguageSkill");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropIndex(
                name: "IX_LanguageSkill_LanguageId",
                table: "LanguageSkill");

            migrationBuilder.DropIndex(
                name: "IX_LanguageSkill_UserId_LanguageId",
                table: "LanguageSkill");

            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "LanguageSkill");

            migrationBuilder.AddColumn<string>(
                name: "LanguageName",
                table: "LanguageSkill",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageSkill_UserId",
                table: "LanguageSkill",
                column: "UserId");
        }
    }
}
