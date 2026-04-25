using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyTime_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingToAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalMeetingId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "Bookings");

            migrationBuilder.AddColumn<Guid>(
                name: "BookingId",
                table: "MeetingAttendances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_MeetingAttendances_BookingId",
                table: "MeetingAttendances",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingAttendances_Bookings_BookingId",
                table: "MeetingAttendances",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingAttendances_Bookings_BookingId",
                table: "MeetingAttendances");

            migrationBuilder.DropIndex(
                name: "IX_MeetingAttendances_BookingId",
                table: "MeetingAttendances");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "MeetingAttendances");

            migrationBuilder.AddColumn<string>(
                name: "ExternalMeetingId",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Platform",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
