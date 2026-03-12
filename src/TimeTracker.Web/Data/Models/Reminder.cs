namespace TimeTracker.Web.Data.Models;

public enum ReminderRepeat { None, Daily, Weekly }
public enum ReminderStatus { Active, Snoozed, Dismissed }

public class Reminder
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime RemindOn { get; set; }
    public ReminderRepeat Repeat { get; set; } = ReminderRepeat.None;
    public ReminderStatus Status { get; set; } = ReminderStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
