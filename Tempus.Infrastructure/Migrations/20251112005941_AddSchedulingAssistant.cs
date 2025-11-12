using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tempus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingAssistant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchedulingPolls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OrganizerEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OrganizerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    AllowMultipleResponses = table.Column<bool>(type: "bit", nullable: false),
                    ShowParticipantNames = table.Column<bool>(type: "bit", nullable: false),
                    RequireLogin = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SelectedTimeSlotId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulingPolls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PollTimeSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchedulingPollId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseCount = table.Column<int>(type: "int", nullable: false),
                    YesCount = table.Column<int>(type: "int", nullable: false),
                    NoCount = table.Column<int>(type: "int", nullable: false),
                    MaybeCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollTimeSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollTimeSlots_SchedulingPolls_SchedulingPollId",
                        column: x => x.SchedulingPollId,
                        principalTable: "SchedulingPolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PollResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchedulingPollId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PollTimeSlotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RespondentEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RespondentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Response = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollResponses_PollTimeSlots_PollTimeSlotId",
                        column: x => x.PollTimeSlotId,
                        principalTable: "PollTimeSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PollResponses_SchedulingPolls_SchedulingPollId",
                        column: x => x.SchedulingPollId,
                        principalTable: "SchedulingPolls",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PollResponses_PollTimeSlotId",
                table: "PollResponses",
                column: "PollTimeSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_PollResponses_SchedulingPollId_RespondentEmail",
                table: "PollResponses",
                columns: new[] { "SchedulingPollId", "RespondentEmail" });

            migrationBuilder.CreateIndex(
                name: "IX_PollTimeSlots_SchedulingPollId",
                table: "PollTimeSlots",
                column: "SchedulingPollId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulingPolls_CreatedAt",
                table: "SchedulingPolls",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulingPolls_OrganizerEmail",
                table: "SchedulingPolls",
                column: "OrganizerEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PollResponses");

            migrationBuilder.DropTable(
                name: "PollTimeSlots");

            migrationBuilder.DropTable(
                name: "SchedulingPolls");
        }
    }
}
