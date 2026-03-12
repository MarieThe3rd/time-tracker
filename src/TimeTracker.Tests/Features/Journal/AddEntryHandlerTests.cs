using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Journal.AddEntry;

namespace TimeTracker.Tests.Features.Journal;

public class AddEntryHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static AddEntryHandler CreateHandler(AppDbContext db) =>
        new AddEntryHandler(new SqlJournalEntryRepository(db));

    [Fact]
    public async Task HandleAsync_ValidInput_SavesEntry()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var input = new AddJournalEntryInput(JournalTypeId: 3, "Shipped the feature", "It was great");

        var entry = await handler.HandleAsync(input);

        Assert.NotNull(entry);
        Assert.Equal(3, entry.JournalTypeId);
        Assert.Equal("Shipped the feature", entry.Title);
        Assert.Equal(1, await db.JournalEntries.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_DefaultsToToday_WhenNoDatProvided()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var input = new AddJournalEntryInput(JournalTypeId: 2, "Learned something", "");

        var entry = await handler.HandleAsync(input);

        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), entry.Date);
    }

    [Fact]
    public async Task HandleAsync_TrimsWhitespace_OnTitleAndBody()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var input = new AddJournalEntryInput(JournalTypeId: 1, "  Title  ", "  Body  ");

        var entry = await handler.HandleAsync(input);

        Assert.Equal("Title", entry.Title);
        Assert.Equal("Body", entry.Body);
    }

    [Fact]
    public async Task HandleAsync_EmptyTitle_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var input = new AddJournalEntryInput(JournalTypeId: 3, "", "body");

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(input));
    }

    [Fact]
    public async Task HandleAsync_WhitespaceOnlyTitle_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var input = new AddJournalEntryInput(JournalTypeId: 3, "   ", "body");

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(input));
    }

    [Theory]
    [InlineData(1)] // Challenge
    [InlineData(2)] // Learning
    [InlineData(3)] // Success
    public async Task HandleAsync_AllSystemJournalTypeIds_Persist(int journalTypeId)
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var input = new AddJournalEntryInput(journalTypeId, "Some title", "Some body");

        var entry = await handler.HandleAsync(input);

        Assert.Equal(journalTypeId, entry.JournalTypeId);
    }

    [Fact]
    public async Task HandleAsync_ExplicitDate_UsesProvidedDate()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var explicitDate = new DateOnly(2025, 12, 31);
        var input = new AddJournalEntryInput(JournalTypeId: 3, "Year-end win", "", Date: explicitDate);

        var entry = await handler.HandleAsync(input);

        Assert.Equal(explicitDate, entry.Date);
    }

    [Fact]
    public async Task HandleAsync_LinkedTimeEntryId_Persisted()
    {
        using var db = CreateDb();
        db.TimeEntries.Add(new TimeEntry
        {
            Id = 99,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var input = new AddJournalEntryInput(
            JournalTypeId: 3, "Good session", "Details", LinkedTimeEntryId: 99);

        var entry = await handler.HandleAsync(input);

        Assert.Equal(99, entry.LinkedTimeEntryId);
    }

    [Fact]
    public async Task HandleAsync_SetsCreatedAt_ToRecentUtcTime()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var before = DateTime.UtcNow.AddSeconds(-1);
        var input = new AddJournalEntryInput(JournalTypeId: 2, "Test", "Body");

        var entry = await handler.HandleAsync(input);

        Assert.True(entry.CreatedAt >= before);
        Assert.True(entry.CreatedAt <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task HandleAsync_JournalCategoryId_Persisted()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var input = new AddJournalEntryInput(JournalTypeId: 3, "Categorized win", "", JournalCategoryId: 7);

        var entry = await handler.HandleAsync(input);

        Assert.Equal(7, entry.JournalCategoryId);
    }
}
