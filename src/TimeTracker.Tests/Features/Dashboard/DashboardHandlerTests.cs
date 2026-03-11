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

    [Fact]
    public async Task HandleAsync_CategoryStats_OrderedByHoursDescending()
    {
        using var db = CreateDb();
        var catA = new WorkCategory { Id = 20, Name = "Alpha", Color = "#000", Icon = "bi-a", IsSystem = false };
        var catB = new WorkCategory { Id = 21, Name = "Beta", Color = "#000", Icon = "bi-b", IsSystem = false };
        db.WorkCategories.AddRange(catA, catB);
        var start = DateTime.Today.ToUniversalTime();
        db.TimeEntries.AddRange(
            // catA: 1 hour
            new TimeEntry { StartTime = start, EndTime = start.AddHours(1), WorkCategoryId = 20 },
            // catB: 3 hours
            new TimeEntry { StartTime = start.AddHours(2), EndTime = start.AddHours(5), WorkCategoryId = 21 }
        );
        await db.SaveChangesAsync();

        var data = await new DashboardHandler(db).HandleAsync();

        Assert.Equal("Beta", data.CategoryStats[0].Name);
        Assert.Equal("Alpha", data.CategoryStats[1].Name);
    }

    [Fact]
    public async Task HandleAsync_RecentEntries_LimitedToFive()
    {
        using var db = CreateDb();
        var start = DateTime.Today.ToUniversalTime();
        for (int i = 0; i < 8; i++)
        {
            db.TimeEntries.Add(new TimeEntry
            {
                StartTime = start.AddHours(i),
                EndTime = start.AddHours(i + 1)
            });
        }
        await db.SaveChangesAsync();

        var data = await new DashboardHandler(db).HandleAsync();

        Assert.Equal(5, data.RecentEntries.Count);
    }

    [Fact]
    public async Task HandleAsync_RecentEntries_OrderedByStartTimeDescending()
    {
        using var db = CreateDb();
        var start = DateTime.Today.ToUniversalTime();
        db.TimeEntries.AddRange(
            new TimeEntry { StartTime = start.AddHours(1), EndTime = start.AddHours(2) },
            new TimeEntry { StartTime = start.AddHours(3), EndTime = start.AddHours(4) }
        );
        await db.SaveChangesAsync();

        var data = await new DashboardHandler(db).HandleAsync();

        Assert.True(data.RecentEntries[0].StartTime > data.RecentEntries[1].StartTime);
    }

    [Fact]
    public async Task HandleAsync_UnratedEntries_DoNotAffectAvgProductivity()
    {
        using var db = CreateDb();
        var start = DateTime.Today.ToUniversalTime();
        db.TimeEntries.AddRange(
            new TimeEntry { StartTime = start, EndTime = start.AddHours(1), ProductivityRating = 4 },
            new TimeEntry { StartTime = start.AddHours(2), EndTime = start.AddHours(3), ProductivityRating = null }
        );
        await db.SaveChangesAsync();

        var data = await new DashboardHandler(db).HandleAsync();

        // Only the rated entry counts — avg should be 4, not 2
        Assert.Equal(4.0, data.AvgProductivity);
    }

    [Fact]
    public async Task HandleAsync_AiEntries_ReturnsTodayAndWeekAiSummary()
    {
        using var db = CreateDb();
        var today = DateTime.Today.ToUniversalTime();
        var monday = today.AddDays(-(((int)DateTime.Today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7));
        var mondayIsToday = monday.Date == today.Date;

        db.TimeEntries.AddRange(
            new TimeEntry
            {
                StartTime = today.AddHours(1),
                EndTime = today.AddHours(2),
                AiUsed = true,
                AiTimeSavedMinutes = 20
            },
            new TimeEntry
            {
                StartTime = monday.AddHours(9),
                EndTime = monday.AddHours(10),
                AiUsed = true,
                AiTimeSavedMinutes = 40
            },
            new TimeEntry
            {
                StartTime = today.AddDays(-10),
                EndTime = today.AddDays(-10).AddHours(1),
                AiUsed = true,
                AiTimeSavedMinutes = 60
            });

        await db.SaveChangesAsync();

        var data = await new DashboardHandler(db).HandleAsync();

        Assert.Equal(mondayIsToday ? 2 : 1, data.AiSummary.TodayAssistedEntries);
        Assert.Equal(mondayIsToday ? 60 : 20, data.AiSummary.TodayTimeSavedMinutes);
        Assert.Equal(2, data.AiSummary.WeekAssistedEntries);
        Assert.Equal(60, data.AiSummary.WeekTimeSavedMinutes);
    }
}
