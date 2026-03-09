using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<WorkCategory> WorkCategories => Set<WorkCategory>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TimeEntryTag> TimeEntryTags => Set<TimeEntryTag>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TimeEntryTag>()
            .HasKey(t => new { t.TimeEntryId, t.TagId });

        modelBuilder.Entity<UserSettings>().HasData(
            new UserSettings { Id = 1, DailyNotesSubfolder = @"Journal\Daily" }
        );

        modelBuilder.Entity<WorkCategory>().HasData(
            new WorkCategory { Id = 1, Name = "Development", Color = "#0d6efd", Icon = "bi-code-slash", IsSystem = true },
            new WorkCategory { Id = 2, Name = "Team Leadership", Color = "#6f42c1", Icon = "bi-people", IsSystem = true },
            new WorkCategory { Id = 3, Name = "Board & Planning", Color = "#0dcaf0", Icon = "bi-kanban", IsSystem = true },
            new WorkCategory { Id = 4, Name = "Business Meetings", Color = "#ffc107", Icon = "bi-calendar-event", IsSystem = true },
            new WorkCategory { Id = 5, Name = "Leadership Reporting", Color = "#fd7e14", Icon = "bi-bar-chart", IsSystem = true },
            new WorkCategory { Id = 6, Name = "Cross-Team / External", Color = "#20c997", Icon = "bi-diagram-3", IsSystem = true },
            new WorkCategory { Id = 7, Name = "Admin / Other", Color = "#6c757d", Icon = "bi-inbox", IsSystem = true }
        );
    }
}
