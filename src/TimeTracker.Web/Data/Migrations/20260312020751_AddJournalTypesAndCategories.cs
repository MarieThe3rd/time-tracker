using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TimeTracker.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalTypesAndCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create JournalTypes table first (needed before FK can reference it)
            migrationBuilder.CreateTable(
                name: "JournalTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalTypes", x => x.Id);
                });

            // 2. Seed system journal types with explicit IDs (1=Challenge, 2=Learning, 3=Success)
            migrationBuilder.InsertData(
                table: "JournalTypes",
                columns: new[] { "Id", "Color", "Icon", "IsSystem", "Name" },
                values: new object[,]
                {
                    { 1, "#ffc107", "bi-lightning-charge", true, "Challenge" },
                    { 2, "#0dcaf0", "bi-mortarboard", true, "Learning" },
                    { 3, "#198754", "bi-trophy", true, "Success" }
                });

            // 3. Rename the old enum column (Type: 0=Challenge,1=Learning,2=Success) to JournalTypeId
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "JournalEntries",
                newName: "JournalTypeId");

            // 4. Shift existing values: old enum (0,1,2) → new IDs (1,2,3)
            migrationBuilder.Sql("UPDATE JournalEntries SET JournalTypeId = JournalTypeId + 1");

            migrationBuilder.AddColumn<int>(
                name: "JournalCategoryId",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JournalCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RemindOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Repeat = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AssignedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliverableTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkCategoryId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskItems_WorkCategories_WorkCategoryId",
                        column: x => x.WorkCategoryId,
                        principalTable: "WorkCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_JournalCategoryId",
                table: "JournalEntries",
                column: "JournalCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_JournalTypeId",
                table: "JournalEntries",
                column: "JournalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_WorkCategoryId",
                table: "TaskItems",
                column: "WorkCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_JournalCategories_JournalCategoryId",
                table: "JournalEntries",
                column: "JournalCategoryId",
                principalTable: "JournalCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_JournalTypes_JournalTypeId",
                table: "JournalEntries",
                column: "JournalTypeId",
                principalTable: "JournalTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_JournalCategories_JournalCategoryId",
                table: "JournalEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_JournalTypes_JournalTypeId",
                table: "JournalEntries");

            migrationBuilder.DropTable(
                name: "JournalCategories");

            migrationBuilder.DropTable(
                name: "JournalTypes");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_JournalCategoryId",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_JournalTypeId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "JournalCategoryId",
                table: "JournalEntries");

            migrationBuilder.RenameColumn(
                name: "JournalTypeId",
                table: "JournalEntries",
                newName: "Type");
        }
    }
}
