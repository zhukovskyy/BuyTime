using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyTime_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixedTimeslotRelation : Migration
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
                column: "TimeslotId");
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
                column: "TimeslotId",
                unique: true);
        }
    }
}
