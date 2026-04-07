using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationMembers_AspNetUsers_UserId",
                table: "ConversationMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_HobbyUsers_AspNetUsers_UserId",
                table: "HobbyUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingAttendances_AspNetUsers_UserId",
                table: "MeetingAttendances");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationMembers_AspNetUsers_UserId",
                table: "ConversationMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HobbyUsers_AspNetUsers_UserId",
                table: "HobbyUsers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingAttendances_AspNetUsers_UserId",
                table: "MeetingAttendances",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationMembers_AspNetUsers_UserId",
                table: "ConversationMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_HobbyUsers_AspNetUsers_UserId",
                table: "HobbyUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingAttendances_AspNetUsers_UserId",
                table: "MeetingAttendances");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationMembers_AspNetUsers_UserId",
                table: "ConversationMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HobbyUsers_AspNetUsers_UserId",
                table: "HobbyUsers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingAttendances_AspNetUsers_UserId",
                table: "MeetingAttendances",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
