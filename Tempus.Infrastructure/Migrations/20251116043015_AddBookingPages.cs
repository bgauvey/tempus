using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tempus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingPages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WelcomeMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    AvailableDurations = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BufferBeforeMinutes = table.Column<int>(type: "int", nullable: false),
                    BufferAfterMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxBookingsPerDay = table.Column<int>(type: "int", nullable: true),
                    MinimumNoticeMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxAdvanceBookingDays = table.Column<int>(type: "int", nullable: false),
                    AvailableDaysOfWeek = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DailyStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    DailyEndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CalendarId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequireGuestName = table.Column<bool>(type: "bit", nullable: false),
                    RequireGuestEmail = table.Column<bool>(type: "bit", nullable: false),
                    RequireGuestPhone = table.Column<bool>(type: "bit", nullable: false),
                    AllowGuestNotes = table.Column<bool>(type: "bit", nullable: false),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    ConfirmationMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RedirectUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SendConfirmationEmail = table.Column<bool>(type: "bit", nullable: false),
                    SendReminderEmail = table.Column<bool>(type: "bit", nullable: false),
                    ReminderMinutesBeforeAppointment = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ShowOnPublicProfile = table.Column<bool>(type: "bit", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncludeVideoConference = table.Column<bool>(type: "bit", nullable: false),
                    VideoConferenceProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TotalBookings = table.Column<int>(type: "int", nullable: false),
                    LastBookingAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingPages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingPages_Calendars_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "Calendars",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingPages_CalendarId",
                table: "BookingPages",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingPages_Slug",
                table: "BookingPages",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingPages_UserId_CreatedAt",
                table: "BookingPages",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingPages_UserId_IsActive",
                table: "BookingPages",
                columns: new[] { "UserId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingPages");
        }
    }
}
