using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DatingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class LocationSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Locations_LocationId",
                table: "Meetings");

            migrationBuilder.AddColumn<bool>(
                name: "ApprovedUpdate",
                table: "MeetingAttendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "Address", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "1052 Budapest, Deák Ferenc tér", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Deák Ferenc tér", null },
                    { 2, "1024 Budapest, Széll Kálmán tér", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Széll Kálmán tér", null },
                    { 3, "1056 Budapest, Fővám tér", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fővám tér", null },
                    { 4, "1146 Budapest, Városliget", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Városliget", null },
                    { 5, "1117 Budapest, Kopaszi-gát", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kopaszi-gát", null }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Locations_LocationId",
                table: "Meetings",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Locations_LocationId",
                table: "Meetings");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "ApprovedUpdate",
                table: "MeetingAttendances");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Locations_LocationId",
                table: "Meetings",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
