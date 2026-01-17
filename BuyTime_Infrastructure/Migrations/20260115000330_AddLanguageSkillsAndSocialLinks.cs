using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyTime_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageSkillsAndSocialLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LanguageSkill",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageSkill", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LanguageSkill_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialLink",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Network = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UrlOrHandle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialLink_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LanguageSkill_UserId",
                table: "LanguageSkill",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialLink_UserId",
                table: "SocialLink",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LanguageSkill");

            migrationBuilder.DropTable(
                name: "SocialLink");
        }
    }
}
