using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Features.Reports.DailyNote;

namespace TimeTracker.Tests.Features.Reports;

public class ReportsHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetRangeDataAsync_ReturnsEntriesInRange()
    {
        using var db = CreateDb();
        var from = new DateOnly(2026, 3, 1);
        var to   = new DateOnly(2026, 3, 7);

        db.TimeEntries.AddRange(
            new TimeEntry
            {
                StartTime = new DateTime(2026, 3, 3, 9, 0, 0, DateTimeKind.Utc),
                EndTime   = new DateTime(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc)
            },
            new TimeEntry
            {
                StartTime = new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc),
                EndTime   = new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc)
            }
        );
        await db.SaveChangesAsync();

        var handler = new ReportsHandler(db);
        var (entries, _) = await handler.GetRangeDataAsync(new ReportRange(from, to));

        Assert.Single(entries);
    }

    [Fact]
    public async Task GetRangeDataAsync_ExcludesEntriesWithNoEndTime()
    {
        using var db = CreateDb();
        db.TimeEntries.Add(new TimeEntry
        {
            StartTime = new DateTime(2026, 3, 3, 9, 0, 0, DateTimeKind.Utc),
            EndTime = null // running timer
        });
        await db.SaveChangesAsync();

        var handler = new ReportsHandler(db);
        var (entries, _) = await handler.GetRangeDataAsync(
            new ReportRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 7)));

        Assert.Empty(entries);
    }

    [Fact]
    public async Task GetRangeDataAsync_ReturnsJournalInRange()
    {
        using var db = CreateDb();
        db.JournalEntries.AddRange(
            new JournalEntry { Date = new DateOnly(2026, 3, 3), Type = JournalEntryType.Success, Title = "In", Body = "", CreatedAt = DateTime.UtcNow },
            new JournalEntry { Date = new DateOnly(2026, 3, 15), Type = JournalEntryType.Success, Title = "Out", Body = "", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var handler = new ReportsHandler(db);
        var (_, journal) = await handler.GetRangeDataAsync(
            new ReportRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 7)));

        Assert.Single(journal);
        Assert.Equal("In", journal[0].Title);
    }
}
