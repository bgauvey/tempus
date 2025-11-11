using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tempus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRSVPFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attendees_EventId",
                table: "Attendees");

            migrationBuilder.AddColumn<bool>(
                name: "AllowProposedTimes",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GuestListVisibility",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RSVPDeadline",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireRSVP",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SendReminderToNonResponders",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOptional",
                table: "Attendees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReminderSent",
                table: "Attendees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReminderCount",
                table: "Attendees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResponseDate",
                table: "Attendees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseNotes",
                table: "Attendees",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProposedTimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttendeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposedStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProposedEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VoteCount = table.Column<int>(type: "int", nullable: false),
                    VotedByEmails = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposedTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposedTimes_Attendees_AttendeeId",
                        column: x => x.AttendeeId,
                        principalTable: "Attendees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendees_EventId_Email",
                table: "Attendees",
                columns: new[] { "EventId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_ProposedTimes_AttendeeId",
                table: "ProposedTimes",
                column: "AttendeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProposedTimes");

            migrationBuilder.DropIndex(
                name: "IX_Attendees_EventId_Email",
                table: "Attendees");

            migrationBuilder.DropColumn(
                name: "AllowProposedTimes",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "GuestListVisibility",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RSVPDeadline",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RequireRSVP",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SendReminderToNonResponders",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsOptional",
                table: "Attendees");

            migrationBuilder.DropColumn(
                name: "LastReminderSent",
                table: "Attendees");

            migrationBuilder.DropColumn(
                name: "ReminderCount",
                table: "Attendees");

            migrationBuilder.DropColumn(
                name: "ResponseDate",
                table: "Attendees");

            migrationBuilder.DropColumn(
                name: "ResponseNotes",
                table: "Attendees");

            migrationBuilder.CreateIndex(
                name: "IX_Attendees_EventId",
                table: "Attendees",
                column: "EventId");
        }
    }
}
