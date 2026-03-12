namespace TimeTracker.Web.Data.Models;

public class JournalCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6c757d";
    public string Icon { get; set; } = "bi-tag";
    public bool IsSystem { get; set; }
}
