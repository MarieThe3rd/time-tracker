using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Features.Reports.AiUsage;

public class AiUsageReportHandler(AppDbContext db)
{
  public async Task<List<AiUsageReportItem>> GetAiUsageAsync(DateTime from, DateTime to)
  {
    return await db.TimeEntries
        .Where(e => e.AiUsed && e.StartTime >= from && e.StartTime <= to)
        .Select(e => new AiUsageReportItem
        {
          Id = e.Id,
          StartTime = e.StartTime,
          EndTime = e.EndTime,
          Description = e.Description,
          AiTimeSavedMinutes = e.AiTimeSavedMinutes,
          AiNotes = e.AiNotes,
          ValueAdded = e.ValueAdded
        })
        .OrderBy(e => e.StartTime)
        .ToListAsync();
  }

  /// <summary>
  /// Returns AI usage aggregated by ISO week (Monday–Sunday), ordered by WeekStart ascending.
  /// Grouping is performed in memory after fetching the filtered flat list from the DB.
  /// </summary>
  public async Task<List<AiUsageWeeklyItem>> GetWeeklyAiUsageAsync(DateTime from, DateTime to)
  {
    var items = await db.TimeEntries
        .Where(e => e.AiUsed && e.StartTime >= from && e.StartTime <= to)
        .Select(e => new AiUsageReportItem
        {
          Id = e.Id,
          StartTime = e.StartTime,
          EndTime = e.EndTime,
          Description = e.Description,
          AiTimeSavedMinutes = e.AiTimeSavedMinutes,
          AiNotes = e.AiNotes,
          ValueAdded = e.ValueAdded
        })
        .OrderBy(e => e.StartTime)
        .ToListAsync();

    return items
        .GroupBy(e => GetWeekStart(e.StartTime))
        .OrderBy(g => g.Key)
        .Select(g => new AiUsageWeeklyItem
        {
          WeekStart = g.Key,
          WeekEnd = g.Key.AddDays(6),
          AiTaskCount = g.Count(),
          TotalTimeSavedMinutes = g.Sum(e => e.AiTimeSavedMinutes ?? 0),
          ValueAdded = string.Join("; ", g
              .Where(e => !string.IsNullOrWhiteSpace(e.ValueAdded))
              .Select(e => e.ValueAdded!)),
          Notes = string.Join("; ", g
              .Where(e => !string.IsNullOrWhiteSpace(e.AiNotes))
              .Select(e => e.AiNotes!))
        })
        .ToList();
  }

  /// <summary>Returns the Monday that starts the ISO week containing <paramref name="dt"/>.</summary>
  private static DateOnly GetWeekStart(DateTime dt)
  {
    var date = DateOnly.FromDateTime(dt);
    int diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
    return date.AddDays(-diff);
  }
}

public class AiUsageReportItem
{
  public int Id { get; set; }
  public DateTime StartTime { get; set; }
  public DateTime? EndTime { get; set; }
  public string? Description { get; set; }
  public int? AiTimeSavedMinutes { get; set; }
  public string? AiNotes { get; set; }
  public string? ValueAdded { get; set; }
}

public class AiUsageWeeklyItem
{
  public DateOnly WeekStart { get; set; }
  public DateOnly WeekEnd { get; set; }
  public int AiTaskCount { get; set; }
  public int TotalTimeSavedMinutes { get; set; }
  /// <summary>Non-empty ValueAdded strings from entries in this week, joined with "; ".</summary>
  public string ValueAdded { get; set; } = string.Empty;
  /// <summary>Non-empty AiNotes strings from entries in this week, joined with "; ".</summary>
  public string Notes { get; set; } = string.Empty;
}
