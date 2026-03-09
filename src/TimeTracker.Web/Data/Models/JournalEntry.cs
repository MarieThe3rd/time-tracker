namespace TimeTracker.Web.Data.Models;

public enum JournalEntryType { Challenge, Learning, Success }

public class JournalEntry
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public JournalEntryType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int? LinkedTimeEntryId { get; set; }
    public TimeEntry? LinkedTimeEntry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
