using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Features.Reports.AiUsage;
using Xunit;

namespace TimeTracker.Tests.Features.Reports;

public class AiUsageReportHandlerTests
{
  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new AppDbContext(options);
  }

  [Fact]
  public async Task GetAiUsageAsync_Returns_Only_AiUsed_Entries_In_Range()
  {
    using var db = CreateDbContext();
    db.TimeEntries.AddRange(new[]
    {
            new TimeEntry { Id = 1, StartTime = DateTime.UtcNow.AddDays(-2), EndTime = DateTime.UtcNow.AddDays(-2).AddHours(1), AiUsed = true, AiTimeSavedMinutes = 10, AiNotes = "Test1" },
            new TimeEntry { Id = 2, StartTime = DateTime.UtcNow.AddDays(-1), EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1), AiUsed = false },
            new TimeEntry { Id = 3, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), AiUsed = true, AiTimeSavedMinutes = 20, AiNotes = "Test2" }
        });
    await db.SaveChangesAsync();
    var handler = new AiUsageReportHandler(db);
    var from = DateTime.UtcNow.AddDays(-3);
    var to = DateTime.UtcNow.AddDays(1);
    var result = await handler.GetAiUsageAsync(from, to);
    Assert.Equal(2, result.Count);
    Assert.All(result, x => Assert.True(x.AiTimeSavedMinutes == 10 || x.AiTimeSavedMinutes == 20));
  }

  // ── GetWeeklyAiUsageAsync tests ──────────────────────────────────────────────

  // 2026-01-05 is a confirmed Monday; use it as the anchor for all weekly tests.
  private static readonly DateTime Monday_2026_01_05 = new(2026, 1, 5, 9, 0, 0, DateTimeKind.Utc);
  private static readonly DateTime Tuesday_2026_01_06 = new(2026, 1, 6, 10, 0, 0, DateTimeKind.Utc);

  [Fact]
  public async Task GetWeeklyAiUsageAsync_SingleWeek_HappyPath()
  {
    using var db = CreateDbContext();
    db.TimeEntries.AddRange(new[]
    {
      new TimeEntry { Id = 1, StartTime = Monday_2026_01_05,  EndTime = Monday_2026_01_05.AddHours(1),  AiUsed = true, AiTimeSavedMinutes = 15 },
      new TimeEntry { Id = 2, StartTime = Tuesday_2026_01_06, EndTime = Tuesday_2026_01_06.AddHours(1), AiUsed = true, AiTimeSavedMinutes = 25 },
    });
    await db.SaveChangesAsync();

    var handler = new AiUsageReportHandler(db);
    var result = await handler.GetWeeklyAiUsageAsync(
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc));

    Assert.Single(result);
    var week = result[0];
    Assert.Equal(2, week.AiTaskCount);
    Assert.Equal(40, week.TotalTimeSavedMinutes);
    Assert.Equal(DayOfWeek.Monday, week.WeekStart.DayOfWeek);
    Assert.Equal(week.WeekStart.AddDays(6), week.WeekEnd);
  }

  [Fact]
  public async Task GetWeeklyAiUsageAsync_MultipleWeeks_OrderedByWeekStart()
  {
    using var db = CreateDbContext();
    // Monday Jan 5 (week 1) and Monday Jan 12 (week 2) — definitely different ISO weeks.
    var week2Monday = new DateTime(2026, 1, 12, 9, 0, 0, DateTimeKind.Utc);
    db.TimeEntries.AddRange(new[]
    {
      new TimeEntry { Id = 1, StartTime = Monday_2026_01_05, EndTime = Monday_2026_01_05.AddHours(1), AiUsed = true, AiTimeSavedMinutes = 10 },
      new TimeEntry { Id = 2, StartTime = week2Monday,       EndTime = week2Monday.AddHours(1),       AiUsed = true, AiTimeSavedMinutes = 20 },
    });
    await db.SaveChangesAsync();

    var handler = new AiUsageReportHandler(db);
    var result = await handler.GetWeeklyAiUsageAsync(
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 1, 18, 0, 0, 0, DateTimeKind.Utc));

    Assert.Equal(2, result.Count);
    Assert.True(result[0].WeekStart < result[1].WeekStart, "Weeks should be ordered by WeekStart ascending.");
    Assert.Equal(new DateOnly(2026, 1, 5),  result[0].WeekStart);
    Assert.Equal(new DateOnly(2026, 1, 12), result[1].WeekStart);
  }

  [Fact]
  public async Task GetWeeklyAiUsageAsync_NoEntries_ReturnsEmptyList()
  {
    using var db = CreateDbContext();
    await db.SaveChangesAsync();

    var handler = new AiUsageReportHandler(db);
    var result = await handler.GetWeeklyAiUsageAsync(
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc));

    Assert.Empty(result);
  }

  [Fact]
  public async Task GetWeeklyAiUsageAsync_NullAiTimeSavedMinutes_CountsAsZero()
  {
    using var db = CreateDbContext();
    db.TimeEntries.Add(
      new TimeEntry { Id = 1, StartTime = Monday_2026_01_05, EndTime = Monday_2026_01_05.AddHours(1), AiUsed = true, AiTimeSavedMinutes = null });
    await db.SaveChangesAsync();

    var handler = new AiUsageReportHandler(db);
    var result = await handler.GetWeeklyAiUsageAsync(
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc));

    Assert.Single(result);
    Assert.Equal(0, result[0].TotalTimeSavedMinutes);
  }

  [Fact]
  public async Task GetWeeklyAiUsageAsync_NotesAndValueAdded_JoinedWithSemicolon()
  {
    using var db = CreateDbContext();
    db.TimeEntries.AddRange(new[]
    {
      new TimeEntry { Id = 1, StartTime = Monday_2026_01_05,  EndTime = Monday_2026_01_05.AddHours(1),  AiUsed = true, AiNotes = "Note A", ValueAdded = "Value A" },
      new TimeEntry { Id = 2, StartTime = Tuesday_2026_01_06, EndTime = Tuesday_2026_01_06.AddHours(1), AiUsed = true, AiNotes = "Note B", ValueAdded = "Value B" },
    });
    await db.SaveChangesAsync();

    var handler = new AiUsageReportHandler(db);
    var result = await handler.GetWeeklyAiUsageAsync(
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc));

    Assert.Single(result);
    var week = result[0];
    Assert.Contains("Note A", week.Notes);
    Assert.Contains("Note B", week.Notes);
    Assert.Contains("; ", week.Notes);
    Assert.Contains("Value A", week.ValueAdded);
    Assert.Contains("Value B", week.ValueAdded);
    Assert.Contains("; ", week.ValueAdded);
  }

  [Fact]
  public async Task GetWeeklyAiUsageAsync_NonAiEntries_AreExcluded()
  {
    using var db = CreateDbContext();
    db.TimeEntries.AddRange(new[]
    {
      new TimeEntry { Id = 1, StartTime = Monday_2026_01_05,  EndTime = Monday_2026_01_05.AddHours(1),  AiUsed = true,  AiTimeSavedMinutes = 30 },
      new TimeEntry { Id = 2, StartTime = Tuesday_2026_01_06, EndTime = Tuesday_2026_01_06.AddHours(1), AiUsed = false, AiTimeSavedMinutes = 99 },
    });
    await db.SaveChangesAsync();

    var handler = new AiUsageReportHandler(db);
    var result = await handler.GetWeeklyAiUsageAsync(
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc));

    Assert.Single(result);
    Assert.Equal(1, result[0].AiTaskCount);
    Assert.Equal(30, result[0].TotalTimeSavedMinutes);
  }

  [Fact]
  public async Task GetWeeklyAiUsageAsync_OutOfRangeEntries_AreExcluded()
  {
    using var db = CreateDbContext();
    // Entry before the range
    var beforeRange = new DateTime(2025, 12, 29, 9, 0, 0, DateTimeKind.Utc);
    // Entry inside the range
    var inRange = Monday_2026_01_05;
    // Entry after the range
    var afterRange = new DateTime(2026, 1, 20, 9, 0, 0, DateTimeKind.Utc);

    db.TimeEntries.AddRange(new[]
    {
      new TimeEntry { Id = 1, StartTime = beforeRange, EndTime = beforeRange.AddHours(1), AiUsed = true, AiTimeSavedMinutes = 100 },
      new TimeEntry { Id = 2, StartTime = inRange,     EndTime = inRange.AddHours(1),     AiUsed = true, AiTimeSavedMinutes = 10  },
      new TimeEntry { Id = 3, StartTime = afterRange,  EndTime = afterRange.AddHours(1),  AiUsed = true, AiTimeSavedMinutes = 200 },
    });
    await db.SaveChangesAsync();

    var handler = new AiUsageReportHandler(db);
    var result = await handler.GetWeeklyAiUsageAsync(
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc));

    Assert.Single(result);
    Assert.Equal(10, result[0].TotalTimeSavedMinutes);
  }
}
