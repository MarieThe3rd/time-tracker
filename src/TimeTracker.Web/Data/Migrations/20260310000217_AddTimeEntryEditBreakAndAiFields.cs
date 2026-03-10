using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeEntryEditBreakAndAiFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiNotes",
                table: "TimeEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AiTimeSavedMinutes",
                table: "TimeEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AiUsed",
                table: "TimeEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBreak",
                table: "TimeEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ValueAdded",
                table: "TimeEntries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiNotes",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "AiTimeSavedMinutes",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "AiUsed",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "IsBreak",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "ValueAdded",
                table: "TimeEntries");
        }
    }
}
