using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tempus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartOfWeek = table.Column<int>(type: "int", nullable: false),
                    TimeFormat = table.Column<int>(type: "int", nullable: false),
                    DateFormat = table.Column<int>(type: "int", nullable: false),
                    ShowWeekNumbers = table.Column<bool>(type: "bit", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultCalendarView = table.Column<int>(type: "int", nullable: false),
                    ShowWeekendInWeekView = table.Column<bool>(type: "bit", nullable: false),
                    TimeSlotDuration = table.Column<int>(type: "int", nullable: false),
                    ScrollToTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    WorkHoursStart = table.Column<TimeSpan>(type: "time", nullable: false),
                    WorkHoursEnd = table.Column<TimeSpan>(type: "time", nullable: false),
                    WeekendDays = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkingDays = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LunchBreakStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    LunchBreakEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    BufferTimeBetweenEvents = table.Column<int>(type: "int", nullable: false),
                    DefaultMeetingDuration = table.Column<int>(type: "int", nullable: false),
                    DefaultEventColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DefaultEventVisibility = table.Column<int>(type: "int", nullable: false),
                    DefaultLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmailNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DesktopNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DefaultReminderTimes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultCalendarId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarSettings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarSettings_UserId",
                table: "CalendarSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarSettings");
        }
    }
}
