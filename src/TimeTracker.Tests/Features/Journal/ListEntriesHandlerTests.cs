using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Features.Journal.ListEntries;

namespace TimeTracker.Tests.Features.Journal;

public class ListEntriesHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<AppDbContext> SeedAsync()
    {
        var db = CreateDb();
        db.JournalEntries.AddRange(
            new JournalEntry { Date = new DateOnly(2026, 3, 1), Type = JournalEntryType.Success, Title = "Win 1", Body = "", CreatedAt = DateTime.UtcNow },
            new JournalEntry { Date = new DateOnly(2026, 3, 2), Type = JournalEntryType.Challenge, Title = "Challenge 1", Body = "", CreatedAt = DateTime.UtcNow },
            new JournalEntry { Date = new DateOnly(2026, 3, 3), Type = JournalEntryType.Learning, Title = "Learned 1", Body = "", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task HandleAsync_NoFilter_ReturnsAll()
    {
        using var db = await SeedAsync();
        var handler = new ListEntriesHandler(db);

        var results = await handler.HandleAsync(new JournalFilter());

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task HandleAsync_FilterByType_ReturnsMatchingOnly()
    {
        using var db = await SeedAsync();
        var handler = new ListEntriesHandler(db);

        var results = await handler.HandleAsync(new JournalFilter(Type: JournalEntryType.Success));

        Assert.Single(results);
        Assert.All(results, e => Assert.Equal(JournalEntryType.Success, e.Type));
    }

    [Fact]
    public async Task HandleAsync_FilterByDateRange_ReturnsMatchingOnly()
    {
        using var db = await SeedAsync();
        var handler = new ListEntriesHandler(db);

        var results = await handler.HandleAsync(new JournalFilter(
            From: new DateOnly(2026, 3, 2),
            To: new DateOnly(2026, 3, 2)));

        Assert.Single(results);
        Assert.Equal("Challenge 1", results[0].Title);
    }
}
