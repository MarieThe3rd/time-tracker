using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Timer;

namespace TimeTracker.Tests.Features.Timer;

public class GetTodayEntriesHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_ReturnsOnlyTodaysEntries()
    {
        using var db = CreateDb();
        // Use local midnight + 10h to guarantee the entry is "today" in any timezone
        var todayUtc = DateTime.Today.AddHours(10).ToUniversalTime();
        var yesterdayUtc = DateTime.Today.AddDays(-1).AddHours(10).ToUniversalTime();

        db.TimeEntries.AddRange(
            new TimeEntry { StartTime = todayUtc, EndTime = todayUtc.AddHours(1) },
            new TimeEntry { StartTime = yesterdayUtc, EndTime = yesterdayUtc.AddHours(1) }
        );
        await db.SaveChangesAsync();

        var handler = new GetTodayEntriesHandler(new SqlTimeEntryRepository(db));
        var entries = await handler.HandleAsync();

        Assert.Single(entries);
    }

    [Fact]
    public async Task HandleAsync_OrdersByStartTimeDescending()
    {
        using var db = CreateDb();
        // Use local midnight + hours to guarantee both entries are "today" in any timezone
        var earlyTodayUtc = DateTime.Today.AddHours(9).ToUniversalTime();
        var laterTodayUtc = DateTime.Today.AddHours(11).ToUniversalTime();
        db.TimeEntries.AddRange(
            new TimeEntry { StartTime = earlyTodayUtc, EndTime = earlyTodayUtc.AddHours(1) },
            new TimeEntry { StartTime = laterTodayUtc, EndTime = laterTodayUtc.AddHours(1) }
        );
        await db.SaveChangesAsync();

        var handler = new GetTodayEntriesHandler(new SqlTimeEntryRepository(db));
        var entries = await handler.HandleAsync();

        Assert.Equal(2, entries.Count);
        Assert.True(entries[0].StartTime > entries[1].StartTime);
    }
}