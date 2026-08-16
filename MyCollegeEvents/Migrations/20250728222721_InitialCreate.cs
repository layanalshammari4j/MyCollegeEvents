using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyCollegeEvents.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventID);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    ParticipantID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UniversityID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventID = table.Column<int>(type: "int", nullable: false),
                    AttendedBefore = table.Column<bool>(type: "bit", nullable: false),
                    WantCertificate = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.ParticipantID);
                    table.ForeignKey(
                        name: "FK_Participants_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "EventID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "EventID", "CreatedBy", "CreatedDate", "Date", "Description", "Title" },
                values: new object[,]
                {
                    { 1, "د. فاطمة أحمد", new DateTime(2025, 7, 29, 1, 27, 21, 372, DateTimeKind.Local).AddTicks(9903), new DateTime(2025, 8, 28, 1, 27, 21, 372, DateTimeKind.Local).AddTicks(9357), "ورشة تعليمية لتعلم أساسيات البرمجة باستخدام C#", "ورشة البرمجة للمبتدئات" },
                    { 2, "د. سارة محمد", new DateTime(2025, 7, 29, 1, 27, 21, 373, DateTimeKind.Local).AddTicks(130), new DateTime(2025, 9, 12, 1, 27, 21, 373, DateTimeKind.Local).AddTicks(125), "محاضرة حول تطبيقات الذكاء الاصطناعي في الحياة العملية", "محاضرة الذكاء الاصطناعي" },
                    { 3, "د. نورا علي", new DateTime(2025, 7, 29, 1, 27, 21, 373, DateTimeKind.Local).AddTicks(132), new DateTime(2025, 9, 27, 1, 27, 21, 373, DateTimeKind.Local).AddTicks(131), "مؤتمر يهدف لتمكين المرأة في مجال التكنولوجيا", "مؤتمر التكنولوجيا النسائي" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Participants_EventID",
                table: "Participants",
                column: "EventID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
