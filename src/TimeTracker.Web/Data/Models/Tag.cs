namespace TimeTracker.Web.Data.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<TimeEntryTag> TimeEntryTags { get; set; } = [];
}
