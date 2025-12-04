using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tempus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeedyMeetingsSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ApplySpeedyMeetingsToShortEvents",
                table: "CalendarSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableSpeedyMeetings",
                table: "CalendarSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SpeedyMeetingsMinutes",
                table: "CalendarSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SpeedyMeetingsThresholdMinutes",
                table: "CalendarSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplySpeedyMeetingsToShortEvents",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "EnableSpeedyMeetings",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "SpeedyMeetingsMinutes",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "SpeedyMeetingsThresholdMinutes",
                table: "CalendarSettings");
        }
    }
}
