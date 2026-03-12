using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Reports.AiUsage;
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

    private static ReportsHandler CreateHandler(AppDbContext db) =>
        new ReportsHandler(new SqlTimeEntryRepository(db), new SqlJournalEntryRepository(db));

    [Fact]
    public async Task GetRangeDataAsync_ReturnsEntriesInRange()
    {
        using var db = CreateDb();
        var from = new DateOnly(2026, 3, 1);
        var to = new DateOnly(2026, 3, 7);

        db.TimeEntries.AddRange(
            new TimeEntry
            {
                StartTime = new DateTime(2026, 3, 3, 9, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc)
            },
            new TimeEntry
            {
                StartTime = new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc)
            }
        );
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
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

        var handler = CreateHandler(db);
        var (entries, _) = await handler.GetRangeDataAsync(
            new ReportRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 7)));

        Assert.Empty(entries);
    }

    [Fact]
    public async Task GetRangeDataAsync_ReturnsJournalInRange()
    {
        using var db = CreateDb();
        db.JournalEntries.AddRange(
            new JournalEntry { Date = new DateOnly(2026, 3, 3), JournalTypeId = 3, Title = "In", Body = "", CreatedAt = DateTime.UtcNow },
            new JournalEntry { Date = new DateOnly(2026, 3, 15), JournalTypeId = 3, Title = "Out", Body = "", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var (_, journal) = await handler.GetRangeDataAsync(
            new ReportRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 7)));

        Assert.Single(journal);
        Assert.Equal("In", journal[0].Title);
    }

    [Fact]
    public async Task GetRangeDataAsync_EntriesOrderedByStartTimeAscending()
    {
        using var db = CreateDb();
        db.TimeEntries.AddRange(
            new TimeEntry { StartTime = new DateTime(2026, 3, 5, 14, 0, 0, DateTimeKind.Utc), EndTime = new DateTime(2026, 3, 5, 15, 0, 0, DateTimeKind.Utc) },
            new TimeEntry { StartTime = new DateTime(2026, 3, 3, 9, 0, 0, DateTimeKind.Utc), EndTime = new DateTime(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc) }
        );
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var (entries, _) = await handler.GetRangeDataAsync(new ReportRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 7)));

        Assert.True(entries[0].StartTime < entries[1].StartTime);
    }

    [Fact]
    public async Task GetRangeDataAsync_JournalOrderedByDateAscending()
    {
        using var db = CreateDb();
        db.JournalEntries.AddRange(
            new JournalEntry { Date = new DateOnly(2026, 3, 7), JournalTypeId = 3, Title = "Later",   Body = "", CreatedAt = DateTime.UtcNow },
            new JournalEntry { Date = new DateOnly(2026, 3, 2), JournalTypeId = 3, Title = "Earlier", Body = "", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var (_, journal) = await handler.GetRangeDataAsync(new ReportRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 7)));

        Assert.Equal("Earlier", journal[0].Title);
        Assert.Equal("Later", journal[1].Title);
    }

    [Fact]
    public async Task GetRangeDataAsync_EmptyDatabase_ReturnsBothEmpty()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var (entries, journal) = await handler.GetRangeDataAsync(new ReportRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 7)));

        Assert.Empty(entries);
        Assert.Empty(journal);
    }

    [Fact]
    public async Task GetWeeklyAiUsageAsync_GroupsAiEntriesByMondayWeekStart()
    {
        using var db = CreateDb();
        db.TimeEntries.AddRange(
            new TimeEntry
            {
                StartTime = new DateTime(2026, 3, 3, 9, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc),
                AiUsed = true,
                AiTimeSavedMinutes = 15,
                AiNotes = "Generated tests",
                ValueAdded = "Faster coverage"
            },
            new TimeEntry
            {
                StartTime = new DateTime(2026, 3, 6, 14, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 3, 6, 15, 0, 0, DateTimeKind.Utc),
                AiUsed = true,
                AiTimeSavedMinutes = 25,
                AiNotes = "Refactoring help",
                ValueAdded = "Cleaner code"
            });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var weeks = await handler.GetWeeklyAiUsageAsync(new ReportRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 7)));

        var week = Assert.Single(weeks);
        Assert.Equal(new DateOnly(2026, 3, 2), week.WeekStart);
        Assert.Equal(new DateOnly(2026, 3, 8), week.WeekEnd);
        Assert.Equal(2, week.AiTaskCount);
        Assert.Equal(40, week.TotalTimeSavedMinutes);
        Assert.Equal(120, week.TotalTimeSpentMinutes);
        Assert.Contains("Faster coverage", week.ValueAdded);
        Assert.Contains("Refactoring help", week.Notes);
    }

    [Fact]
    public async Task GetWeeklyAiUsageAsync_IgnoresNonAiAndRunningEntries()
    {
        using var db = CreateDb();
        db.TimeEntries.AddRange(
            new TimeEntry
            {
                StartTime = new DateTime(2026, 3, 3, 9, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc),
                AiUsed = false,
                AiTimeSavedMinutes = 15
            },
            new TimeEntry
            {
                StartTime = new DateTime(2026, 3, 4, 9, 0, 0, DateTimeKind.Utc),
                EndTime = null,
                AiUsed = true,
                AiTimeSavedMinutes = 20
            });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var weeks = await handler.GetWeeklyAiUsageAsync(new ReportRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 7)));

        Assert.Empty(weeks);
    }

    [Fact]
    public async Task GetWeeklyAiUsageAsync_ComputesSpentMinutesPerWeek()
    {
        using var db = CreateDb();
        db.TimeEntries.AddRange(
            new TimeEntry
            {
                StartTime = new DateTime(2026, 3, 2, 9, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 3, 2, 9, 45, 0, DateTimeKind.Utc),
                AiUsed = true,
                AiTimeSavedMinutes = 10
            },
            new TimeEntry
            {
                StartTime = new DateTime(2026, 3, 9, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 3, 9, 11, 30, 0, DateTimeKind.Utc),
                AiUsed = true,
                AiTimeSavedMinutes = 20
            });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var weeks = await handler.GetWeeklyAiUsageAsync(new ReportRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 14)));

        Assert.Equal(2, weeks.Count);
        Assert.Equal(new DateOnly(2026, 3, 2), weeks[0].WeekStart);
        Assert.Equal(45, weeks[0].TotalTimeSpentMinutes);
        Assert.Equal(new DateOnly(2026, 3, 9), weeks[1].WeekStart);
        Assert.Equal(90, weeks[1].TotalTimeSpentMinutes);
    }
}

