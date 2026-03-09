using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Features.Dashboard;

namespace TimeTracker.Tests.Features.Dashboard;

public class DashboardHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_NoEntries_ReturnsZeroTotals()
    {
        using var db = CreateDb();
        var handler = new DashboardHandler(db);

        var data = await handler.HandleAsync();

        Assert.Equal(TimeSpan.Zero, data.TotalToday);
        Assert.Equal(0, data.AvgProductivity);
        Assert.Empty(data.CategoryStats);
        Assert.Empty(data.RecentEntries);
    }

    [Fact]
    public async Task HandleAsync_TodayEntries_SumsDurationCorrectly()
    {
        using var db = CreateDb();
        var start = DateTime.Today.ToUniversalTime();
        db.TimeEntries.AddRange(
            new TimeEntry { StartTime = start, EndTime = start.AddHours(2) },
            new TimeEntry { StartTime = start.AddHours(3), EndTime = start.AddHours(4) }
        );
        await db.SaveChangesAsync();

        var data = await new DashboardHandler(db).HandleAsync();

        Assert.Equal(3, data.TotalToday.TotalHours);
    }

    [Fact]
    public async Task HandleAsync_YesterdayEntries_NotIncludedInTotal()
    {
        using var db = CreateDb();
        var yesterday = DateTime.Today.AddDays(-1).ToUniversalTime();
        db.TimeEntries.Add(new TimeEntry { StartTime = yesterday, EndTime = yesterday.AddHours(3) });
        await db.SaveChangesAsync();

        var data = await new DashboardHandler(db).HandleAsync();

        Assert.Equal(TimeSpan.Zero, data.TotalToday);
    }

    [Fact]
    public async Task HandleAsync_RatedEntries_AveragesProductivity()
    {
        using var db = CreateDb();
        var start = DateTime.Today.ToUniversalTime();
        db.TimeEntries.AddRange(
            new TimeEntry { StartTime = start, EndTime = start.AddHours(1), ProductivityRating = 4 },
            new TimeEntry { StartTime = start.AddHours(2), EndTime = start.AddHours(3), ProductivityRating = 2 }
        );
        await db.SaveChangesAsync();

        var data = await new DashboardHandler(db).HandleAsync();

        Assert.Equal(3.0, data.AvgProductivity);
    }

    [Fact]
    public async Task HandleAsync_CategoryStats_GroupsByCategory()
    {
        using var db = CreateDb();
        var cat = new WorkCategory { Id = 10, Name = "Dev", Color = "#000", Icon = "bi-code", IsSystem = false };
        db.WorkCategories.Add(cat);
        var start = DateTime.Today.ToUniversalTime();
        db.TimeEntries.AddRange(
            new TimeEntry { StartTime = start, EndTime = start.AddHours(1), WorkCategoryId = 10 },
            new TimeEntry { StartTime = start.AddHours(2), EndTime = start.AddHours(3), WorkCategoryId = 10 }
        );
        await db.SaveChangesAsync();

        var data = await new DashboardHandler(db).HandleAsync();

        Assert.Single(data.CategoryStats);
        Assert.Equal("Dev", data.CategoryStats[0].Name);
        Assert.Equal(2.0, data.CategoryStats[0].TotalHours);
    }

    [Fact]
    public async Task HandleAsync_RecentJournal_ReturnsLatestThree()
    {
        using var db = CreateDb();
        for (int i = 1; i <= 5; i++)
        {
            db.JournalEntries.Add(new JournalEntry
            {
                Date = new DateOnly(2026, 3, i),
                Type = JournalEntryType.Success,
                Title = $"Entry {i}",
                Body = "",
                CreatedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();

        var data = await new DashboardHandler(db).HandleAsync();

        Assert.Equal(3, data.RecentJournal.Count);
        Assert.Equal("Entry 5", data.RecentJournal[0].Title);
    }
}
