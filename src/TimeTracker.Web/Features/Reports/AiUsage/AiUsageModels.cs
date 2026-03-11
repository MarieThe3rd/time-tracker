namespace TimeTracker.Web.Features.Reports.AiUsage;

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
  public string ValueAdded { get; set; } = string.Empty;
  public string Notes { get; set; } = string.Empty;
}