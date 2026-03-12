using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Journal.UpdateJournalEntry;

namespace TimeTracker.Tests.Features.Journal;

public class UpdateJournalEntryHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    private static UpdateJournalEntryHandler CreateHandler(AppDbContext db) =>
        new UpdateJournalEntryHandler(new SqlJournalEntryRepository(db), new SqlJournalCategoryRepository(db));

    private static async Task<JournalEntry> SeedEntryAsync(AppDbContext db, int journalTypeId = 3)
    {
        var entry = new JournalEntry
        {
            JournalTypeId = journalTypeId,
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
    public async Task HandleAsync_ValidInput_UpdatesEntry()
    {
        using var db = CreateDb();
        var entry = await SeedEntryAsync(db);
        var handler = CreateHandler(db);

        var input = new UpdateJournalEntryInput(
            entry.Id, JournalTypeId: 1, Title: "Updated Title",
            Body: "Updated body", Date: new DateOnly(2026, 3, 15));

        var result = await handler.HandleAsync(input);

        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated body", result.Body);
        Assert.Equal(1, result.JournalTypeId);
        Assert.Equal(new DateOnly(2026, 3, 15), result.Date);
    }

    [Fact]
    public async Task HandleAsync_ValidInput_PersistsChangesToDb()
    {
        using var db = CreateDb();
        var entry = await SeedEntryAsync(db);
        var handler = CreateHandler(db);

        await handler.HandleAsync(new UpdateJournalEntryInput(
            entry.Id, JournalTypeId: 2, Title: "Persisted Title",
            Body: "Persisted body", Date: new DateOnly(2026, 5, 1)));

        var fromDb = await db.JournalEntries.FindAsync(entry.Id);
        Assert.Equal("Persisted Title", fromDb!.Title);
        Assert.Equal(2, fromDb.JournalTypeId);
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(new UpdateJournalEntryInput(
                999, JournalTypeId: 1, Title: "Title",
                Body: "", Date: DateOnly.FromDateTime(DateTime.Today))));
    }

    [Fact]
    public async Task HandleAsync_EmptyTitle_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var entry = await SeedEntryAsync(db);
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new UpdateJournalEntryInput(
                entry.Id, JournalTypeId: 1, Title: "",
                Body: "", Date: DateOnly.FromDateTime(DateTime.Today))));
    }

    [Fact]
    public async Task HandleAsync_WhitespaceTitle_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var entry = await SeedEntryAsync(db);
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new UpdateJournalEntryInput(
                entry.Id, JournalTypeId: 1, Title: "   ",
                Body: "", Date: DateOnly.FromDateTime(DateTime.Today))));
    }

    [Fact]
    public async Task HandleAsync_TrimsWhitespace_OnTitleAndBody()
    {
        using var db = CreateDb();
        var entry = await SeedEntryAsync(db);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new UpdateJournalEntryInput(
            entry.Id, JournalTypeId: 1, Title: "  Trimmed Title  ",
            Body: "  Trimmed body  ", Date: new DateOnly(2026, 3, 1)));

        Assert.Equal("Trimmed Title", result.Title);
        Assert.Equal("Trimmed body", result.Body);
    }

    [Fact]
    public async Task HandleAsync_UpdatesLinkedTimeEntryId()
    {
        using var db = CreateDb();
        db.TimeEntries.Add(new TimeEntry
        {
            Id = 42,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var entry = await SeedEntryAsync(db);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new UpdateJournalEntryInput(
            entry.Id, JournalTypeId: 3, Title: "Linked entry",
            Body: "", Date: new DateOnly(2026, 3, 1), LinkedTimeEntryId: 42));

        Assert.Equal(42, result.LinkedTimeEntryId);
    }

    [Fact]
    public async Task HandleAsync_UpdatesJournalCategoryId()
    {
        using var db = CreateDb();
        db.JournalCategories.Add(new JournalCategory { Id = 5, Name = "Focus", Color = "#fff", Icon = "bi-tag" });
        await db.SaveChangesAsync();

        var entry = await SeedEntryAsync(db);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new UpdateJournalEntryInput(
            entry.Id, JournalTypeId: 3, Title: "Categorized entry",
            Body: "", Date: new DateOnly(2026, 3, 1), JournalCategoryId: 5));

        Assert.Equal(5, result.JournalCategoryId);
    }
}
