using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DatingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedUserAndAdminRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1", null, "Admin", "ADMIN" },
                    { "2", null, "User", "USER" }
                });

            migrationBuilder.Sql(@"
                INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
                SELECT [Id], N'2'
                FROM [AspNetUsers] AS [users]
                WHERE NOT EXISTS (
                    SELECT 1 FROM [AspNetUserRoles] AS [roles]
                    WHERE [roles].[UserId] = [users].[Id] AND [roles].[RoleId] = N'2'
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2");
        }
    }
}
