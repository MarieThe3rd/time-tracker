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
          AiNotes = e.AiNotes
        })
        .OrderBy(e => e.StartTime)
        .ToListAsync();
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
}
