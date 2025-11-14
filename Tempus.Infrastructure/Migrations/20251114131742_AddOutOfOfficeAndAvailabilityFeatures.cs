using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tempus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutOfOfficeAndAvailabilityFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowMeetingsDuringLunchBreak",
                table: "CalendarSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoDeclineMeetingsOutsideWorkingHours",
                table: "CalendarSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableFocusTimeProtection",
                table: "CalendarSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxMeetingsPerDay",
                table: "CalendarSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinimumNoticePeriodHours",
                table: "CalendarSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OutOfOfficeStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AutoResponderMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SendAutoResponder = table.Column<bool>(type: "bit", nullable: false),
                    AutoDeclineMeetings = table.Column<bool>(type: "bit", nullable: false),
                    DeclineOptionalOnly = table.Column<bool>(type: "bit", nullable: false),
                    ExemptOrganizerEmails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShowAsBusy = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DeclineMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutOfOfficeStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutOfOfficeStatuses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutOfOfficeStatuses_StartDate_EndDate_IsActive",
                table: "OutOfOfficeStatuses",
                columns: new[] { "StartDate", "EndDate", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_OutOfOfficeStatuses_UserId_IsActive",
                table: "OutOfOfficeStatuses",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_OutOfOfficeStatuses_UserId_StartDate_EndDate",
                table: "OutOfOfficeStatuses",
                columns: new[] { "UserId", "StartDate", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutOfOfficeStatuses");

            migrationBuilder.DropColumn(
                name: "AllowMeetingsDuringLunchBreak",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "AutoDeclineMeetingsOutsideWorkingHours",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "EnableFocusTimeProtection",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "MaxMeetingsPerDay",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "MinimumNoticePeriodHours",
                table: "CalendarSettings");
        }
    }
}
