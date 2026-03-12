using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<WorkCategory> WorkCategories => Set<WorkCategory>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalType> JournalTypes => Set<JournalType>();
    public DbSet<JournalCategory> JournalCategories => Set<JournalCategory>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TimeEntryTag> TimeEntryTags => Set<TimeEntryTag>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Reminder> Reminders => Set<Reminder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TimeEntryTag>()
            .HasKey(t => new { t.TimeEntryId, t.TagId });

        modelBuilder.Entity<JournalEntry>()
            .HasOne(e => e.JournalType)
            .WithMany()
            .HasForeignKey(e => e.JournalTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<JournalEntry>()
            .HasOne(e => e.JournalCategory)
            .WithMany()
            .HasForeignKey(e => e.JournalCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<UserSettings>().HasData(
            new UserSettings { Id = 1, DailyNotesSubfolder = @"Journal\Daily", WeeklyNotesSubfolder = @"Journal\Weekly" }
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

        modelBuilder.Entity<JournalType>().HasData(
            new JournalType { Id = 1, Name = "Challenge", Color = "#ffc107", Icon = "bi-lightning-charge", IsSystem = true },
            new JournalType { Id = 2, Name = "Learning",  Color = "#0dcaf0", Icon = "bi-mortarboard",     IsSystem = true },
            new JournalType { Id = 3, Name = "Success",   Color = "#198754", Icon = "bi-trophy",          IsSystem = true }
        );

        modelBuilder.Entity<JournalCategory>().HasData(
            new JournalCategory { Id = 1, Name = "Work",     Color = "#0d6efd", Icon = "bi-briefcase", IsSystem = true },
            new JournalCategory { Id = 2, Name = "Personal", Color = "#6f42c1", Icon = "bi-person",    IsSystem = true },
            new JournalCategory { Id = 3, Name = "Learning", Color = "#198754", Icon = "bi-book",      IsSystem = true },
            new JournalCategory { Id = 4, Name = "Health",   Color = "#dc3545", Icon = "bi-heart",     IsSystem = true }
        );
    }
}
