namespace TimeTracker.Web.Data.Models;

public class JournalType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6c757d";
    public string Icon { get; set; } = "bi-journal";
    public bool IsSystem { get; set; }
}
