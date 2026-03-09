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
}
