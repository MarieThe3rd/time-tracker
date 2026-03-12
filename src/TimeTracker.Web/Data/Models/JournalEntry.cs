namespace TimeTracker.Web.Data.Models;

public class JournalEntry
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int JournalTypeId { get; set; }
    public JournalType? JournalType { get; set; }
    public int? JournalCategoryId { get; set; }
    public JournalCategory? JournalCategory { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int? LinkedTimeEntryId { get; set; }
    public TimeEntry? LinkedTimeEntry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

