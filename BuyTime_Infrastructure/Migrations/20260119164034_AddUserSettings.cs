using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyTime_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShowCurrencyEquivalent = table.Column<bool>(type: "bit", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NotifyInTelegram = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnBooking = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnFinance = table.Column<bool>(type: "bit", nullable: false),
                    NotifyReminders = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnNewFeedback = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettings");
        }
    }
}
