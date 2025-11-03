using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tempus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarViewPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CalendarEndHour",
                table: "CalendarSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CalendarStartHour",
                table: "CalendarSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "CompactView",
                table: "CalendarSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HiddenEventTypes",
                table: "CalendarSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastUsedView",
                table: "CalendarSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastViewChangeDate",
                table: "CalendarSettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RememberLastView",
                table: "CalendarSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowCancelledEvents",
                table: "CalendarSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowCompletedTasks",
                table: "CalendarSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowEventColors",
                table: "CalendarSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowEventIcons",
                table: "CalendarSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalendarEndHour",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "CalendarStartHour",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "CompactView",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "HiddenEventTypes",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "LastUsedView",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "LastViewChangeDate",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "RememberLastView",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "ShowCancelledEvents",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "ShowCompletedTasks",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "ShowEventColors",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "ShowEventIcons",
                table: "CalendarSettings");
        }
    }
}
