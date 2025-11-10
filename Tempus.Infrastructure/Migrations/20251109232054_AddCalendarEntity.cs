using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tempus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_UserId",
                table: "Events");

            migrationBuilder.AddColumn<Guid>(
                name: "CalendarId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Calendars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    DefaultEventColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DefaultEventVisibility = table.Column<int>(type: "int", nullable: true),
                    DefaultLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultReminderTimes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultMeetingDuration = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calendars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calendars_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_CalendarId",
                table: "Events",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_UserId_CalendarId",
                table: "Events",
                columns: new[] { "UserId", "CalendarId" });

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_UserId_IsDefault",
                table: "Calendars",
                columns: new[] { "UserId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_UserId_SortOrder",
                table: "Calendars",
                columns: new[] { "UserId", "SortOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Calendars_CalendarId",
                table: "Events",
                column: "CalendarId",
                principalTable: "Calendars",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Calendars_CalendarId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "Calendars");

            migrationBuilder.DropIndex(
                name: "IX_Events_CalendarId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_UserId_CalendarId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CalendarId",
                table: "Events");

            migrationBuilder.CreateIndex(
                name: "IX_Events_UserId",
                table: "Events",
                column: "UserId");
        }
    }
}
