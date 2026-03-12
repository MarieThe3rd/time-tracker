using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Journal.ManageCategories;

namespace TimeTracker.Tests.Features.Journal;

public class ManageJournalCategoriesHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    // SQLite is required for tests that call NullCategoryAsync (uses ExecuteUpdateAsync,
    // which the InMemory provider does not support).
    private static (AppDbContext db, SqliteConnection conn) CreateSqliteDb()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return (db, conn);
    }

    private static ManageJournalCategoriesHandler CreateHandler(AppDbContext db) =>
        new ManageJournalCategoriesHandler(new SqlJournalCategoryRepository(db), new SqlJournalEntryRepository(db));

    [Fact]
    public async Task GetAll_returns_all_categories()
    {
        using var db = CreateDb();
        db.JournalCategories.AddRange(
            new JournalCategory { Id = 1, Name = "Work", Color = "#fff", Icon = "bi-briefcase" },
            new JournalCategory { Id = 2, Name = "Personal", Color = "#000", Icon = "bi-person" }
        );
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var result = await handler.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Add_with_valid_name_creates_category()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.AddAsync("  Focus  ", "#ff0000", "bi-lightning");

        Assert.Equal("Focus", result.Name);
        Assert.Equal("#ff0000", result.Color);
        Assert.Equal("bi-lightning", result.Icon);
        Assert.Equal(1, await db.JournalCategories.CountAsync());
    }

    [Fact]
    public async Task Add_with_empty_name_throws()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.AddAsync("", "#000", "bi-x"));

        Assert.Equal(0, await db.JournalCategories.CountAsync());
    }

    [Fact]
    public async Task Add_with_whitespace_name_throws()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.AddAsync("   ", "#000", "bi-x"));

        Assert.Equal(0, await db.JournalCategories.CountAsync());
    }

    [Fact]
    public async Task Update_with_valid_name_patches_and_saves()
    {
        using var db = CreateDb();
        db.JournalCategories.Add(new JournalCategory
        {
            Id = 10, Name = "Old Name", Color = "#000", Icon = "bi-tag"
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        await handler.UpdateAsync(new JournalCategory
        {
            Id = 10, Name = "  New Name  ", Color = "#abc", Icon = "bi-star"
        });

        var updated = await db.JournalCategories.FindAsync(10);
        Assert.Equal("New Name", updated!.Name);
        Assert.Equal("#abc", updated.Color);
        Assert.Equal("bi-star", updated.Icon);
    }

    [Fact]
    public async Task Update_with_empty_name_throws()
    {
        using var db = CreateDb();
        db.JournalCategories.Add(new JournalCategory
        {
            Id = 20, Name = "Valid", Color = "#000", Icon = "bi-tag"
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.UpdateAsync(new JournalCategory { Id = 20, Name = "", Color = "#000", Icon = "bi-tag" }));
    }

    [Fact]
    public async Task Update_with_nonexistent_id_is_noop()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var ex = await Record.ExceptionAsync(() =>
            handler.UpdateAsync(new JournalCategory { Id = 999, Name = "Ghost", Color = "#000", Icon = "bi-x" }));

        Assert.Null(ex);
    }

    [Fact]
    public async Task Delete_delegates_to_repository()
    {
        var (db, conn) = CreateSqliteDb();
        using (conn) using (db)
        {
            db.JournalCategories.Add(new JournalCategory
            {
                Id = 30, Name = "ToDelete", Color = "#000", Icon = "bi-trash", IsSystem = false
            });
            await db.SaveChangesAsync();

            var handler = CreateHandler(db);
            await handler.DeleteAsync(30);

            Assert.Null(await db.JournalCategories.FindAsync(30));
        }
    }

    [Fact]
    public async Task Delete_system_category_throws_InvalidOperationException()
    {
        var (db, conn) = CreateSqliteDb();
        using (conn) using (db)
        {
            // Seed provides Id=1 "Work" with IsSystem=true — use it directly.
            var handler = CreateHandler(db);

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.DeleteAsync(1));
            Assert.NotNull(await db.JournalCategories.FindAsync(1));
        }
    }

    [Fact]
    public async Task Delete_nulls_out_entries_referencing_category()
    {
        var (db, conn) = CreateSqliteDb();
        using (conn) using (db)
        {
            // Add a non-system category beyond the 4 seeded system ones.
            db.JournalCategories.Add(new JournalCategory { Id = 10, Name = "Custom", Color = "#fff", Icon = "bi-tag", IsSystem = false });
            // JournalTypeId=1 exists in seed data.
            var entry = new JournalEntry
            {
                JournalTypeId = 1,
                JournalCategoryId = 10,
                Title = "Entry with category",
                Body = "",
                Date = new DateOnly(2026, 1, 1),
                CreatedAt = DateTime.UtcNow
            };
            db.JournalEntries.Add(entry);
            await db.SaveChangesAsync();

            var handler = CreateHandler(db);
            await handler.DeleteAsync(10);

            db.ChangeTracker.Clear();
            var fromDb = await db.JournalEntries.FindAsync(entry.Id);
            Assert.Null(fromDb!.JournalCategoryId);
            Assert.Null(await db.JournalCategories.FindAsync(10));
        }
    }

    [Fact]
    public async Task Delete_with_nonexistent_id_is_noop_with_cascade()
    {
        var (db, conn) = CreateSqliteDb();
        using (conn) using (db)
        {
            var handler = CreateHandler(db);

            var ex = await Record.ExceptionAsync(() => handler.DeleteAsync(999));

            Assert.Null(ex);
        }
    }
}
