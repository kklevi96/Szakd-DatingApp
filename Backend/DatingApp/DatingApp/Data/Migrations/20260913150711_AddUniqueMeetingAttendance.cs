using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueMeetingAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MeetingAttendances_MeetingId",
                table: "MeetingAttendances");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingAttendances_MeetingId_UserId",
                table: "MeetingAttendances",
                columns: new[] { "MeetingId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MeetingAttendances_MeetingId_UserId",
                table: "MeetingAttendances");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingAttendances_MeetingId",
                table: "MeetingAttendances",
                column: "MeetingId");
        }
    }
}
