using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Journal.UpdateJournalEntry;

namespace TimeTracker.Tests.Features.Journal;

public class UpdateEntryHandlerCategoryValidationTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static UpdateJournalEntryHandler CreateHandler(AppDbContext db) =>
        new UpdateJournalEntryHandler(new SqlJournalEntryRepository(db), new SqlJournalCategoryRepository(db));

    private static async Task<JournalEntry> SeedEntryAsync(AppDbContext db, int? categoryId = null)
    {
        var entry = new JournalEntry
        {
            JournalTypeId = 1,
            JournalCategoryId = categoryId,
            Title = "Original Title",
            Body = "Original body",
            Date = new DateOnly(2026, 1, 1),
            CreatedAt = DateTime.UtcNow
        };
        db.JournalEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    [Fact]
    public async Task Update_with_valid_category_id_saves_category()
    {
        using var db = CreateDb();
        db.JournalCategories.Add(new JournalCategory { Id = 4, Name = "Learning", Color = "#abc", Icon = "bi-book" });
        var entry = await SeedEntryAsync(db);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new UpdateJournalEntryInput(
            entry.Id, JournalTypeId: 1, Title: "Updated",
            Body: "", Date: new DateOnly(2026, 2, 1), JournalCategoryId: 4));

        Assert.Equal(4, result.JournalCategoryId);
        var fromDb = await db.JournalEntries.FindAsync(entry.Id);
        Assert.Equal(4, fromDb!.JournalCategoryId);
    }

    [Fact]
    public async Task Update_with_invalid_category_id_nulls_out_category()
    {
        using var db = CreateDb();
        var entry = await SeedEntryAsync(db, categoryId: null);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new UpdateJournalEntryInput(
            entry.Id, JournalTypeId: 1, Title: "Updated",
            Body: "", Date: new DateOnly(2026, 2, 1), JournalCategoryId: 888));

        Assert.Null(result.JournalCategoryId);
        var fromDb = await db.JournalEntries.FindAsync(entry.Id);
        Assert.Null(fromDb!.JournalCategoryId);
    }

    [Fact]
    public async Task Update_with_null_category_id_clears_category()
    {
        using var db = CreateDb();
        db.JournalCategories.Add(new JournalCategory { Id = 2, Name = "Personal", Color = "#000", Icon = "bi-person" });
        var entry = await SeedEntryAsync(db, categoryId: 2);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new UpdateJournalEntryInput(
            entry.Id, JournalTypeId: 1, Title: "Updated",
            Body: "", Date: new DateOnly(2026, 2, 1), JournalCategoryId: null));

        Assert.Null(result.JournalCategoryId);
        var fromDb = await db.JournalEntries.FindAsync(entry.Id);
        Assert.Null(fromDb!.JournalCategoryId);
    }
}
