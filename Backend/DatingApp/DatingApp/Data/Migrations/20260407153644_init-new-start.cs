using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class initnewstart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Compatibilities_UserId_A",
                table: "Compatibilities");

            migrationBuilder.CreateIndex(
                name: "IX_Compatibilities_UserId_A_UserId_B",
                table: "Compatibilities",
                columns: new[] { "UserId_A", "UserId_B" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Compatibilities_UserId_A_UserId_B",
                table: "Compatibilities");

            migrationBuilder.CreateIndex(
                name: "IX_Compatibilities_UserId_A",
                table: "Compatibilities",
                column: "UserId_A");
        }
    }
}
