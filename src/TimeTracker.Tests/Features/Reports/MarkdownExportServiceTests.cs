using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Features.Reports;

namespace TimeTracker.Tests.Features.Reports;

public class MarkdownExportServiceTests
{
    private readonly MarkdownExportService _svc = new();

    [Fact]
    public void BuildDailyNote_ContainsFrontmatter()
    {
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [], [], 0);
        Assert.Contains("---", md);
        Assert.Contains("date: 2026-03-09", md);
        Assert.Contains("tags: [time-tracker]", md);
    }

    [Fact]
    public void BuildDailyNote_ContainsTimeSectionHeading()
    {
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [], [], 0);
        Assert.Contains("## ⏱ Time Tracker", md);
    }

    [Fact]
    public void BuildDailyNote_WithEntries_RendersTable()
    {
        var cat = new WorkCategory { Id = 1, Name = "Dev", Color = "#000", Icon = "", IsSystem = false };
        var entry = new TimeEntry
        {
            StartTime = new DateTime(2026, 3, 9, 9, 0, 0, DateTimeKind.Utc),
            EndTime   = new DateTime(2026, 3, 9, 11, 0, 0, DateTimeKind.Utc),
            WorkCategory = cat,
            Description = "Sprint planning",
            ProductivityRating = 4
        };
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [entry], [], 4.0);
        Assert.Contains("| Time | Category |", md);
        Assert.Contains("Sprint planning", md);
        Assert.Contains("Dev", md);
    }

    [Fact]
    public void BuildDailyNote_WithJournal_RendersJournalSection()
    {
        var journal = new JournalEntry
        {
            Date = new DateOnly(2026, 3, 9),
            Type = JournalEntryType.Success,
            Title = "Shipped the feature",
            Body = "After 3 sprints, finally done.",
            CreatedAt = DateTime.UtcNow
        };
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [], [journal], 0);
        Assert.Contains("### Journal", md);
        Assert.Contains("Shipped the feature", md);
        Assert.Contains("🏆", md);
    }

    [Fact]
    public void BuildWeeklySummary_ContainsCorrectPeriod()
    {
        var from = new DateOnly(2026, 3, 2);
        var to   = new DateOnly(2026, 3, 8);
        var md = _svc.BuildWeeklySummary(from, to, [], []);
        Assert.Contains("Mar 2", md);
        Assert.Contains("Mar 8", md);
        Assert.Contains("tags: [time-tracker, weekly-summary]", md);
    }

    [Fact]
    public void BuildReviewExport_GroupsByType()
    {
        var entries = new List<JournalEntry>
        {
            new() { Date = new DateOnly(2026, 1, 1), Type = JournalEntryType.Success,   Title = "Win",       Body = "", CreatedAt = DateTime.UtcNow },
            new() { Date = new DateOnly(2026, 1, 2), Type = JournalEntryType.Challenge, Title = "Hard bug",  Body = "", CreatedAt = DateTime.UtcNow },
            new() { Date = new DateOnly(2026, 1, 3), Type = JournalEntryType.Learning,  Title = "Learned X", Body = "", CreatedAt = DateTime.UtcNow },
        };
        var md = _svc.BuildReviewExport(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), entries);
        Assert.Contains("🏆", md);
        Assert.Contains("⚡", md);
        Assert.Contains("🎓", md);
        Assert.Contains("Win", md);
        Assert.Contains("Hard bug", md);
        Assert.Contains("Learned X", md);
    }

    [Fact]
    public void BuildDailyNote_AvgProductivity_IncludedWhenNonZero()
    {
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [], [], 4.5);
        Assert.Contains("4.5/5", md);
    }

    [Fact]
    public void BuildDailyNote_NoProductivity_Omitted()
    {
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [], [], 0);
        Assert.DoesNotContain("/5", md);
    }
}
