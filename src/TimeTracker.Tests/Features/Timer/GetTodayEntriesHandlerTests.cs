using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
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
        var today = DateTime.UtcNow;
        var yesterday = today.AddDays(-1);

        db.TimeEntries.AddRange(
            new TimeEntry { StartTime = today, EndTime = today.AddHours(1) },
            new TimeEntry { StartTime = yesterday, EndTime = yesterday.AddHours(1) }
        );
        await db.SaveChangesAsync();

        var handler = new GetTodayEntriesHandler(db);
        var entries = await handler.HandleAsync();

        Assert.Single(entries);
    }

    [Fact]
    public async Task HandleAsync_OrdersByStartTimeDescending()
    {
        using var db = CreateDb();
        var now = DateTime.UtcNow;
        db.TimeEntries.AddRange(
            new TimeEntry { StartTime = now.AddHours(-3), EndTime = now.AddHours(-2) },
            new TimeEntry { StartTime = now.AddHours(-1), EndTime = now }
        );
        await db.SaveChangesAsync();

        var handler = new GetTodayEntriesHandler(db);
        var entries = await handler.HandleAsync();

        Assert.True(entries[0].StartTime > entries[1].StartTime);
    }
}
