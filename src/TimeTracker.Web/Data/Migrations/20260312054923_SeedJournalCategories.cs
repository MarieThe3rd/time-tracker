using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TimeTracker.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedJournalCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use conditional inserts to avoid duplicate-key errors if rows already exist.
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [JournalCategories] ON;
                IF NOT EXISTS (SELECT 1 FROM [JournalCategories] WHERE [Id] = 1)
                    INSERT INTO [JournalCategories] ([Id],[Color],[Icon],[IsSystem],[Name]) VALUES (1,'#0d6efd','bi-briefcase',1,'Work');
                IF NOT EXISTS (SELECT 1 FROM [JournalCategories] WHERE [Id] = 2)
                    INSERT INTO [JournalCategories] ([Id],[Color],[Icon],[IsSystem],[Name]) VALUES (2,'#6f42c1','bi-person',1,'Personal');
                IF NOT EXISTS (SELECT 1 FROM [JournalCategories] WHERE [Id] = 3)
                    INSERT INTO [JournalCategories] ([Id],[Color],[Icon],[IsSystem],[Name]) VALUES (3,'#198754','bi-book',1,'Learning');
                IF NOT EXISTS (SELECT 1 FROM [JournalCategories] WHERE [Id] = 4)
                    INSERT INTO [JournalCategories] ([Id],[Color],[Icon],[IsSystem],[Name]) VALUES (4,'#dc3545','bi-heart',1,'Health');
                SET IDENTITY_INSERT [JournalCategories] OFF;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "JournalCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "JournalCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "JournalCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "JournalCategories",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
