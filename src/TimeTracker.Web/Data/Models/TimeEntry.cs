namespace TimeTracker.Web.Data.Models;

public class TimeEntry
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? WorkCategoryId { get; set; }
    public WorkCategory? WorkCategory { get; set; }
    public string? Description { get; set; }
    public int? ProductivityRating { get; set; } // 1-5
    public ICollection<TimeEntryTag> TimeEntryTags { get; set; } = [];

    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
}
