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

    [Fact]
    public async Task HandleAsync_FromAfterTo_ThrowsArgumentException()
    {
        using var db = await SeedAsync();
        var handler = new ListEntriesHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(new JournalFilter(
            From: new DateOnly(2026, 3, 5),
            To: new DateOnly(2026, 3, 1))));
    }

    [Fact]
    public async Task HandleAsync_CombinedTypeAndDateFilter_ReturnsMatchingOnly()
    {
        using var db = await SeedAsync();
        var handler = new ListEntriesHandler(db);

        // Only Success entries from Mar 1-2 (Win 1 is Success on Mar 1; Challenge on Mar 2 is not Success)
        var results = await handler.HandleAsync(new JournalFilter(
            Type: JournalEntryType.Success,
            From: new DateOnly(2026, 3, 1),
            To: new DateOnly(2026, 3, 2)));

        Assert.Single(results);
        Assert.Equal("Win 1", results[0].Title);
    }

    [Fact]
    public async Task HandleAsync_NoEntriesMatchFilter_ReturnsEmpty()
    {
        using var db = await SeedAsync();
        var handler = new ListEntriesHandler(db);

        var results = await handler.HandleAsync(new JournalFilter(
            From: new DateOnly(2030, 1, 1),
            To: new DateOnly(2030, 1, 31)));

        Assert.Empty(results);
    }

    [Fact]
    public async Task HandleAsync_ResultsOrderedByDateDescending()
    {
        using var db = await SeedAsync();
        var handler = new ListEntriesHandler(db);

        var results = await handler.HandleAsync(new JournalFilter());

        // Dates should be descending: Mar 3, Mar 2, Mar 1
        Assert.True(results[0].Date >= results[1].Date);
        Assert.True(results[1].Date >= results[2].Date);
    }

    [Fact]
    public async Task HandleAsync_SecondarySort_SameDate_OrdersByCreatedAtDescending()
    {
        using var db = CreateDb();
        var date = new DateOnly(2026, 5, 1);
        var older = DateTime.UtcNow.AddMinutes(-10);
        var newer = DateTime.UtcNow;
        db.JournalEntries.AddRange(
            new JournalEntry { Date = date, Type = JournalEntryType.Success, Title = "First added", Body = "", CreatedAt = older },
            new JournalEntry { Date = date, Type = JournalEntryType.Success, Title = "Second added", Body = "", CreatedAt = newer }
        );
        await db.SaveChangesAsync();
        var handler = new ListEntriesHandler(db);

        var results = await handler.HandleAsync(new JournalFilter());

        Assert.Equal("Second added", results[0].Title);
        Assert.Equal("First added", results[1].Title);
    }
}
