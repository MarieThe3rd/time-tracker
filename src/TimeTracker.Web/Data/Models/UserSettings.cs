namespace TimeTracker.Web.Data.Models;

public class UserSettings
{
    public int Id { get; set; }
    public string? VaultRootPath { get; set; }
    public string DailyNotesSubfolder { get; set; } = @"Journal\Daily";
    public string WeeklyNotesSubfolder { get; set; } = @"Journal\Weekly";
}
