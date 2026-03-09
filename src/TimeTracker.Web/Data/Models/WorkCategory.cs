namespace TimeTracker.Web.Data.Models;

public class WorkCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6c757d";
    public string Icon { get; set; } = "bi-circle";
    public bool IsSystem { get; set; }
    public ICollection<TimeEntry> TimeEntries { get; set; } = [];
}
