using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyTime_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationsAndRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_TimeslotId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TimeslotId",
                table: "Bookings",
                column: "TimeslotId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_TimeslotId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TimeslotId",
                table: "Bookings",
                column: "TimeslotId");
        }
    }
}
