using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyTime_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiWalletSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletAddress",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "WalletType",
                table: "Wallets");

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedAt",
                table: "Wallets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Wallets",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Network",
                table: "Wallets",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_Network_Address",
                table: "Wallets",
                columns: new[] { "Network", "Address" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wallets_Network_Address",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "AddedAt",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "Network",
                table: "Wallets");

            migrationBuilder.AddColumn<string>(
                name: "WalletAddress",
                table: "Wallets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WalletType",
                table: "Wallets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
