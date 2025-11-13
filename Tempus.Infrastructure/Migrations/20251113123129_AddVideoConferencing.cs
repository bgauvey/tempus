using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tempus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoConferencing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VideoConferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    MeetingUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MeetingId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Passcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DialInNumbers = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    DialInPasscode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HostKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExternalMeetingId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoConferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoConferences_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoConferences_EventId",
                table: "VideoConferences",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoConferences");
        }
    }
}
