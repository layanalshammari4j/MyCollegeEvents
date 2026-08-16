using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCollegeEvents.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventID",
                keyValue: 1,
                columns: new[] { "CreatedDate", "Date" },
                values: new object[] { new DateTime(2025, 7, 28, 12, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 9, 15, 10, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventID",
                keyValue: 2,
                columns: new[] { "CreatedDate", "Date" },
                values: new object[] { new DateTime(2025, 7, 28, 12, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 10, 1, 14, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventID",
                keyValue: 3,
                columns: new[] { "CreatedDate", "Date" },
                values: new object[] { new DateTime(2025, 7, 28, 12, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 10, 15, 9, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventID",
                keyValue: 1,
                columns: new[] { "CreatedDate", "Date" },
                values: new object[] { new DateTime(2025, 7, 29, 1, 27, 21, 372, DateTimeKind.Local).AddTicks(9903), new DateTime(2025, 8, 28, 1, 27, 21, 372, DateTimeKind.Local).AddTicks(9357) });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventID",
                keyValue: 2,
                columns: new[] { "CreatedDate", "Date" },
                values: new object[] { new DateTime(2025, 7, 29, 1, 27, 21, 373, DateTimeKind.Local).AddTicks(130), new DateTime(2025, 9, 12, 1, 27, 21, 373, DateTimeKind.Local).AddTicks(125) });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventID",
                keyValue: 3,
                columns: new[] { "CreatedDate", "Date" },
                values: new object[] { new DateTime(2025, 7, 29, 1, 27, 21, 373, DateTimeKind.Local).AddTicks(132), new DateTime(2025, 9, 27, 1, 27, 21, 373, DateTimeKind.Local).AddTicks(131) });
        }
    }
}
