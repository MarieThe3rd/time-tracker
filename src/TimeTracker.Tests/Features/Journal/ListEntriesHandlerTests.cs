using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
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
            new JournalEntry { Date = new DateOnly(2026, 3, 1), JournalTypeId = 3, Title = "Win 1",       Body = "", CreatedAt = DateTime.UtcNow },
            new JournalEntry { Date = new DateOnly(2026, 3, 2), JournalTypeId = 1, Title = "Challenge 1", Body = "", CreatedAt = DateTime.UtcNow },
            new JournalEntry { Date = new DateOnly(2026, 3, 3), JournalTypeId = 2, Title = "Learned 1",   Body = "", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task HandleAsync_NoFilter_ReturnsAll()
    {
        using var db = await SeedAsync();
        var handler = new ListEntriesHandler(new SqlJournalEntryRepository(db));

        var results = await handler.HandleAsync(new JournalFilter());

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task HandleAsync_FilterByJournalTypeId_ReturnsMatchingOnly()
    {
        using var db = await SeedAsync();
        var handler = new ListEntriesHandler(new SqlJournalEntryRepository(db));

        var results = await handler.HandleAsync(new JournalFilter(JournalTypeId: 3));

        Assert.Single(results);
        Assert.All(results, e => Assert.Equal(3, e.JournalTypeId));
    }

    [Fact]
    public async Task HandleAsync_FilterByDateRange_ReturnsMatchingOnly()
    {
        using var db = await SeedAsync();
        var handler = new ListEntriesHandler(new SqlJournalEntryRepository(db));

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
        var handler = new ListEntriesHandler(new SqlJournalEntryRepository(db));

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(new JournalFilter(
            From: new DateOnly(2026, 3, 5),
            To: new DateOnly(2026, 3, 1))));
    }

    [Fact]
    public async Task HandleAsync_CombinedTypeAndDateFilter_ReturnsMatchingOnly()
    {
        using var db = await SeedAsync();
        var handler = new ListEntriesHandler(new SqlJournalEntryRepository(db));

        var results = await handler.HandleAsync(new JournalFilter(
            JournalTypeId: 3,
            From: new DateOnly(2026, 3, 1),
            To: new DateOnly(2026, 3, 2)));

        Assert.Single(results);
        Assert.Equal("Win 1", results[0].Title);
    }

    [Fact]
    public async Task HandleAsync_NoEntriesMatchFilter_ReturnsEmpty()
    {
        using var db = await SeedAsync();
        var handler = new ListEntriesHandler(new SqlJournalEntryRepository(db));

        var results = await handler.HandleAsync(new JournalFilter(
            From: new DateOnly(2030, 1, 1),
            To: new DateOnly(2030, 1, 31)));

        Assert.Empty(results);
    }

    [Fact]
    public async Task HandleAsync_ResultsOrderedByDateDescending()
    {
        using var db = await SeedAsync();
        var handler = new ListEntriesHandler(new SqlJournalEntryRepository(db));

        var results = await handler.HandleAsync(new JournalFilter());

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
            new JournalEntry { Date = date, JournalTypeId = 3, Title = "First added",  Body = "", CreatedAt = older },
            new JournalEntry { Date = date, JournalTypeId = 3, Title = "Second added", Body = "", CreatedAt = newer }
        );
        await db.SaveChangesAsync();
        var handler = new ListEntriesHandler(new SqlJournalEntryRepository(db));

        var results = await handler.HandleAsync(new JournalFilter());

        Assert.Equal("Second added", results[0].Title);
        Assert.Equal("First added", results[1].Title);
    }

    [Fact]
    public async Task HandleAsync_FilterByCategoryId_ReturnsMatchingOnly()
    {
        using var db = CreateDb();
        db.JournalEntries.AddRange(
            new JournalEntry { Date = new DateOnly(2026, 4, 1), JournalTypeId = 1, JournalCategoryId = 10, Title = "Cat 10 entry", Body = "", CreatedAt = DateTime.UtcNow },
            new JournalEntry { Date = new DateOnly(2026, 4, 2), JournalTypeId = 2, JournalCategoryId = 20, Title = "Cat 20 entry", Body = "", CreatedAt = DateTime.UtcNow },
            new JournalEntry { Date = new DateOnly(2026, 4, 3), JournalTypeId = 3, JournalCategoryId = null, Title = "No cat entry", Body = "", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();
        var handler = new ListEntriesHandler(new SqlJournalEntryRepository(db));

        var results = await handler.HandleAsync(new JournalFilter(CategoryId: 10));

        Assert.Single(results);
        Assert.Equal("Cat 10 entry", results[0].Title);
    }

    [Fact]
    public async Task HandleAsync_FilterByJournalTypeIdAndCategoryId_ReturnsIntersection()
    {
        using var db = CreateDb();
        db.JournalEntries.AddRange(
            new JournalEntry { Date = new DateOnly(2026, 4, 1), JournalTypeId = 1, JournalCategoryId = 10, Title = "Match",    Body = "", CreatedAt = DateTime.UtcNow },
            new JournalEntry { Date = new DateOnly(2026, 4, 2), JournalTypeId = 1, JournalCategoryId = 20, Title = "No cat",   Body = "", CreatedAt = DateTime.UtcNow },
            new JournalEntry { Date = new DateOnly(2026, 4, 3), JournalTypeId = 2, JournalCategoryId = 10, Title = "No type",  Body = "", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();
        var handler = new ListEntriesHandler(new SqlJournalEntryRepository(db));

        var results = await handler.HandleAsync(new JournalFilter(JournalTypeId: 1, CategoryId: 10));

        Assert.Single(results);
        Assert.Equal("Match", results[0].Title);
    }
}


