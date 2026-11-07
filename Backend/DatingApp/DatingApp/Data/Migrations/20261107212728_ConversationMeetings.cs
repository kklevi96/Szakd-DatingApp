using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConversationMeetings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConversationId",
                table: "Meetings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_ConversationId",
                table: "Meetings",
                column: "ConversationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Conversations_ConversationId",
                table: "Meetings",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Conversations_ConversationId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_ConversationId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "Meetings");
        }
    }
}
