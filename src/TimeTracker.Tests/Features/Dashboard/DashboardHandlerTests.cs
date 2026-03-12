using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
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

    private static DashboardHandler CreateHandler(AppDbContext db) =>
        new DashboardHandler(
            new SqlTimeEntryRepository(db),
            new SqlJournalEntryRepository(db),
            new SqlTaskItemRepository(db),
            new SqlReminderRepository(db));

    private static DateOnly GetWeekStart(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }

    [Fact]
    public async Task HandleAsync_NoEntries_ReturnsZeroTotals()
    {
        using var db = CreateDb();
        var data = await CreateHandler(db).HandleAsync();

        Assert.Equal(TimeSpan.Zero, data.TotalTime);
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

        var data = await CreateHandler(db).HandleAsync();

        Assert.Equal(3, data.TotalTime.TotalHours);
    }

    [Fact]
    public async Task HandleAsync_YesterdayEntries_NotIncludedInTotal()
    {
        using var db = CreateDb();
        var yesterday = DateTime.Today.AddDays(-1).ToUniversalTime();
        db.TimeEntries.Add(new TimeEntry { StartTime = yesterday, EndTime = yesterday.AddHours(3) });
        await db.SaveChangesAsync();

        var data = await CreateHandler(db).HandleAsync();

        Assert.Equal(TimeSpan.Zero, data.TotalTime);
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

        var data = await CreateHandler(db).HandleAsync();

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

        var data = await CreateHandler(db).HandleAsync();

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
                JournalTypeId = 3,
                Title = $"Entry {i}",
                Body = "",
                CreatedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();

        var data = await CreateHandler(db).HandleAsync();

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

        var data = await CreateHandler(db).HandleAsync();

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

        var data = await CreateHandler(db).HandleAsync();

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

        var data = await CreateHandler(db).HandleAsync();

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

        var data = await CreateHandler(db).HandleAsync();

        // Only the rated entry counts — avg should be 4, not 2
        Assert.Equal(4.0, data.AvgProductivity);
    }

    [Fact]
    public async Task HandleAsync_WithDateRange_ScopesAiSummaryToRange()
    {
        using var db = CreateDb();
        var rangeStart = new DateOnly(2025, 1, 6);  // Monday
        var rangeEnd = new DateOnly(2025, 1, 10);   // Friday

        var startUtc = rangeStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).ToUniversalTime();

        db.TimeEntries.AddRange(
            new TimeEntry
            {
                StartTime = startUtc.AddHours(9),
                EndTime = startUtc.AddHours(10),
                AiUsed = true,
                AiTimeSavedMinutes = 20
            },
            new TimeEntry
            {
                StartTime = startUtc.AddDays(2).AddHours(9),
                EndTime = startUtc.AddDays(2).AddHours(10),
                AiUsed = true,
                AiTimeSavedMinutes = 40
            },
            // outside range — should not be counted
            new TimeEntry
            {
                StartTime = startUtc.AddDays(-10),
                EndTime = startUtc.AddDays(-10).AddHours(1),
                AiUsed = true,
                AiTimeSavedMinutes = 60
            });

        await db.SaveChangesAsync();

        var data = await CreateHandler(db).HandleAsync(rangeStart, rangeEnd);

        Assert.Equal(2, data.AiSummary.AssistedEntries);
        Assert.Equal(60, data.AiSummary.TimeSavedMinutes);
    }

    [Fact]
    public async Task HandleAsync_WithDateRange_ScopesEntriesToRange()
    {
        using var db = CreateDb();
        var rangeStart = new DateOnly(2025, 3, 1);
        var rangeEnd = new DateOnly(2025, 3, 7);

        var inRangeUtc = rangeStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).ToUniversalTime();
        var outOfRangeUtc = rangeStart.AddDays(-1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).ToUniversalTime();

        db.TimeEntries.AddRange(
            new TimeEntry { StartTime = inRangeUtc.AddHours(9), EndTime = inRangeUtc.AddHours(11) },
            new TimeEntry { StartTime = inRangeUtc.AddDays(3).AddHours(9), EndTime = inRangeUtc.AddDays(3).AddHours(10) },
            new TimeEntry { StartTime = outOfRangeUtc.AddHours(9), EndTime = outOfRangeUtc.AddHours(10) }
        );
        await db.SaveChangesAsync();

        var data = await CreateHandler(db).HandleAsync(rangeStart, rangeEnd);

        Assert.Equal(2, data.EntryCount);
        Assert.Equal(3.0, data.TotalTime.TotalHours);
    }

    [Fact]
    public async Task HandleAsync_NoTasks_ReturnsZeroOverdueCount()
    {
        using var db = CreateDb();
        var data = await CreateHandler(db).HandleAsync();

        Assert.Equal(0, data.OverdueTaskCount);
    }

    [Fact]
    public async Task HandleAsync_TasksWithPastDueDate_ReturnsCorrectOverdueCount()
    {
        using var db = CreateDb();
        var yesterday = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
        db.TaskItems.AddRange(
            new TaskItem { Title = "Overdue A", DueDate = yesterday, Status = TaskItemStatus.NotStarted },
            new TaskItem { Title = "Overdue B", DueDate = yesterday, Status = TaskItemStatus.InProgress }
        );
        await db.SaveChangesAsync();

        var data = await CreateHandler(db).HandleAsync();

        Assert.Equal(2, data.OverdueTaskCount);
    }

    [Fact]
    public async Task HandleAsync_CompletedTaskWithPastDueDate_NotCountedAsOverdue()
    {
        using var db = CreateDb();
        var yesterday = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
        db.TaskItems.Add(new TaskItem { Title = "Done task", DueDate = yesterday, Status = TaskItemStatus.Done });
        await db.SaveChangesAsync();

        var data = await CreateHandler(db).HandleAsync();

        Assert.Equal(0, data.OverdueTaskCount);
    }

    [Fact]
    public async Task HandleAsync_NoReminders_ReturnsZeroUpcomingReminderCount()
    {
        using var db = CreateDb();
        var data = await CreateHandler(db).HandleAsync();

        Assert.Equal(0, data.UpcomingReminderCount);
    }

    [Fact]
    public async Task HandleAsync_ActiveRemindersWithinWindow_ReturnsCorrectUpcomingCount()
    {
        using var db = CreateDb();
        var soon = DateTime.UtcNow.AddHours(1);
        db.Reminders.AddRange(
            new Reminder { Title = "Soon A", RemindOn = soon, Status = ReminderStatus.Active },
            new Reminder { Title = "Soon B", RemindOn = soon.AddHours(10), Status = ReminderStatus.Snoozed }
        );
        await db.SaveChangesAsync();

        var data = await CreateHandler(db).HandleAsync();

        Assert.Equal(2, data.UpcomingReminderCount);
    }

    [Fact]
    public async Task HandleAsync_DismissedReminders_NotCountedAsUpcoming()
    {
        using var db = CreateDb();
        var soon = DateTime.UtcNow.AddHours(2);
        db.Reminders.Add(new Reminder { Title = "Dismissed", RemindOn = soon, Status = ReminderStatus.Dismissed });
        await db.SaveChangesAsync();

        var data = await CreateHandler(db).HandleAsync();

        Assert.Equal(0, data.UpcomingReminderCount);
    }
}

