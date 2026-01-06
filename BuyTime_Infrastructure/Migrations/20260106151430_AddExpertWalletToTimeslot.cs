using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyTime_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpertWalletToTimeslot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpertWalletAddress",
                table: "Timeslots",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpertWalletAddress",
                table: "Timeslots");
        }
    }
}
