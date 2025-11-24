using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tempus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFreeBusySharingSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FreeBusyLookAheadDays",
                table: "CalendarSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FreeBusySharingLevel",
                table: "CalendarSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "PublishFreeBusyInformation",
                table: "CalendarSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowPrivateEventsAsBusy",
                table: "CalendarSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreeBusyLookAheadDays",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "FreeBusySharingLevel",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "PublishFreeBusyInformation",
                table: "CalendarSettings");

            migrationBuilder.DropColumn(
                name: "ShowPrivateEventsAsBusy",
                table: "CalendarSettings");
        }
    }
}
