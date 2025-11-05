using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tempus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleCalendarIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "CalendarIntegrations",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CalendarId",
                table: "CalendarIntegrations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "CalendarIntegrations",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SyncEnabled",
                table: "CalendarIntegrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SyncToken",
                table: "CalendarIntegrations",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CalendarIntegrations",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarIntegrations_UserId_Provider",
                table: "CalendarIntegrations",
                columns: new[] { "UserId", "Provider" });

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarIntegrations_AspNetUsers_UserId",
                table: "CalendarIntegrations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarIntegrations_AspNetUsers_UserId",
                table: "CalendarIntegrations");

            migrationBuilder.DropIndex(
                name: "IX_CalendarIntegrations_UserId_Provider",
                table: "CalendarIntegrations");

            migrationBuilder.DropColumn(
                name: "SyncEnabled",
                table: "CalendarIntegrations");

            migrationBuilder.DropColumn(
                name: "SyncToken",
                table: "CalendarIntegrations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CalendarIntegrations");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "CalendarIntegrations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CalendarId",
                table: "CalendarIntegrations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "CalendarIntegrations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
