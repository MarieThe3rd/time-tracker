using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
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

    [Fact]
    public async Task HandleAsync_ValidInput_SavesEntry()
    {
        using var db = CreateDb();
        var handler = new AddEntryHandler(db);
        var input = new AddJournalEntryInput(JournalEntryType.Success, "Shipped the feature", "It was great");

        var entry = await handler.HandleAsync(input);

        Assert.NotNull(entry);
        Assert.Equal(JournalEntryType.Success, entry.Type);
        Assert.Equal("Shipped the feature", entry.Title);
        Assert.Equal(1, await db.JournalEntries.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_DefaultsToToday_WhenNoDatProvided()
    {
        using var db = CreateDb();
        var handler = new AddEntryHandler(db);
        var input = new AddJournalEntryInput(JournalEntryType.Learning, "Learned something", "");

        var entry = await handler.HandleAsync(input);

        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), entry.Date);
    }

    [Fact]
    public async Task HandleAsync_TrimsWhitespace_OnTitleAndBody()
    {
        using var db = CreateDb();
        var handler = new AddEntryHandler(db);
        var input = new AddJournalEntryInput(JournalEntryType.Challenge, "  Title  ", "  Body  ");

        var entry = await handler.HandleAsync(input);

        Assert.Equal("Title", entry.Title);
        Assert.Equal("Body", entry.Body);
    }

    [Fact]
    public async Task HandleAsync_EmptyTitle_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = new AddEntryHandler(db);
        var input = new AddJournalEntryInput(JournalEntryType.Success, "", "body");

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(input));
    }

    [Fact]
    public async Task HandleAsync_WhitespaceOnlyTitle_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = new AddEntryHandler(db);
        var input = new AddJournalEntryInput(JournalEntryType.Success, "   ", "body");

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(input));
    }

    [Theory]
    [InlineData(JournalEntryType.Challenge)]
    [InlineData(JournalEntryType.Learning)]
    [InlineData(JournalEntryType.Success)]
    public async Task HandleAsync_AllJournalTypes_Persist(JournalEntryType type)
    {
        using var db = CreateDb();
        var handler = new AddEntryHandler(db);
        var input = new AddJournalEntryInput(type, "Some title", "Some body");

        var entry = await handler.HandleAsync(input);

        Assert.Equal(type, entry.Type);
    }

    [Fact]
    public async Task HandleAsync_ExplicitDate_UsesProvidedDate()
    {
        using var db = CreateDb();
        var handler = new AddEntryHandler(db);
        var explicitDate = new DateOnly(2025, 12, 31);
        var input = new AddJournalEntryInput(JournalEntryType.Success, "Year-end win", "", Date: explicitDate);

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

        var handler = new AddEntryHandler(db);
        var input = new AddJournalEntryInput(
            JournalEntryType.Success, "Good session", "Details", LinkedTimeEntryId: 99);

        var entry = await handler.HandleAsync(input);

        Assert.Equal(99, entry.LinkedTimeEntryId);
    }

    [Fact]
    public async Task HandleAsync_SetsCreatedAt_ToRecentUtcTime()
    {
        using var db = CreateDb();
        var handler = new AddEntryHandler(db);
        var before = DateTime.UtcNow.AddSeconds(-1);
        var input = new AddJournalEntryInput(JournalEntryType.Learning, "Test", "Body");

        var entry = await handler.HandleAsync(input);

        Assert.True(entry.CreatedAt >= before);
        Assert.True(entry.CreatedAt <= DateTime.UtcNow.AddSeconds(1));
    }
}
