namespace TimeTracker.Web.Data.Models;

public class TimeEntryTag
{
    public int TimeEntryId { get; set; }
    public TimeEntry TimeEntry { get; set; } = null!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
