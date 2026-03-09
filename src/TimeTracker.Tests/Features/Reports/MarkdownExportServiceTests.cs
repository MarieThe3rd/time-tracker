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

    [Fact]
    public void BuildDailyNote_NoEntries_EmptyEntriesSection()
    {
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [], [], 0);
        Assert.DoesNotContain("| Time |", md);
    }

    [Fact]
    public void BuildDailyNote_NoJournal_NoJournalSection()
    {
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [], [], 0);
        Assert.DoesNotContain("### Journal", md);
    }

    [Fact]
    public void BuildDailyNote_EntryWithNullCategory_RendersPlaceholder()
    {
        var entry = new TimeEntry
        {
            StartTime = new DateTime(2026, 3, 9, 10, 0, 0, DateTimeKind.Utc),
            EndTime   = new DateTime(2026, 3, 9, 11, 0, 0, DateTimeKind.Utc),
            WorkCategory = null,
            Description = "Solo work"
        };
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [entry], [], 0);
        Assert.Contains("—", md); // null category placeholder
    }

    [Fact]
    public void BuildDailyNote_EntryWithNoRating_RendersPlaceholder()
    {
        var entry = new TimeEntry
        {
            StartTime = new DateTime(2026, 3, 9, 10, 0, 0, DateTimeKind.Utc),
            EndTime   = new DateTime(2026, 3, 9, 11, 0, 0, DateTimeKind.Utc),
            ProductivityRating = null
        };
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [entry], [], 0);
        // Rating column should contain "—" for null
        var lines = md.Split('\n');
        var dataRow = lines.FirstOrDefault(l => l.StartsWith("| ") && !l.Contains("Time |"));
        Assert.NotNull(dataRow);
        Assert.Contains("—", dataRow);
    }

    [Fact]
    public void BuildDailyNote_JournalAllTypes_ShowCorrectEmojis()
    {
        var entries = new List<JournalEntry>
        {
            new() { Date = new DateOnly(2026,3,9), Type = JournalEntryType.Challenge, Title = "Hard thing", Body = "", CreatedAt = DateTime.UtcNow },
            new() { Date = new DateOnly(2026,3,9), Type = JournalEntryType.Learning,  Title = "Learned",    Body = "", CreatedAt = DateTime.UtcNow },
            new() { Date = new DateOnly(2026,3,9), Type = JournalEntryType.Success,   Title = "Win",        Body = "", CreatedAt = DateTime.UtcNow },
        };
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [], entries, 0);
        Assert.Contains("⚡", md);
        Assert.Contains("🎓", md);
        Assert.Contains("🏆", md);
    }

    [Fact]
    public void BuildWeeklySummary_WithEntries_ShowsCategoryBreakdown()
    {
        var cat = new WorkCategory { Id = 1, Name = "Development", Color = "#000", Icon = "", IsSystem = false };
        var entries = new List<TimeEntry>
        {
            new() { StartTime = new DateTime(2026,3,2,9,0,0,DateTimeKind.Utc), EndTime = new DateTime(2026,3,2,11,0,0,DateTimeKind.Utc), WorkCategory = cat },
            new() { StartTime = new DateTime(2026,3,3,9,0,0,DateTimeKind.Utc), EndTime = new DateTime(2026,3,3,10,0,0,DateTimeKind.Utc), WorkCategory = cat },
        };
        var md = _svc.BuildWeeklySummary(new DateOnly(2026,3,2), new DateOnly(2026,3,8), entries, []);
        Assert.Contains("Development", md);
        Assert.Contains("3h 0m", md);
    }

    [Fact]
    public void BuildWeeklySummary_WithWins_ShowsWinsSection()
    {
        var journal = new List<JournalEntry>
        {
            new() { Date = new DateOnly(2026,3,5), Type = JournalEntryType.Success, Title = "Big win", Body = "Details", CreatedAt = DateTime.UtcNow }
        };
        var md = _svc.BuildWeeklySummary(new DateOnly(2026,3,2), new DateOnly(2026,3,8), [], journal);
        Assert.Contains("### 🏆 Wins", md);
        Assert.Contains("Big win", md);
    }

    [Fact]
    public void BuildWeeklySummary_WithChallenges_ShowsChallengesSection()
    {
        var journal = new List<JournalEntry>
        {
            new() { Date = new DateOnly(2026,3,5), Type = JournalEntryType.Challenge, Title = "Blocked", Body = "", CreatedAt = DateTime.UtcNow }
        };
        var md = _svc.BuildWeeklySummary(new DateOnly(2026,3,2), new DateOnly(2026,3,8), [], journal);
        Assert.Contains("### ⚡ Challenges", md);
    }

    [Fact]
    public void BuildWeeklySummary_WithLearnings_ShowsLearningsSection()
    {
        var journal = new List<JournalEntry>
        {
            new() { Date = new DateOnly(2026,3,5), Type = JournalEntryType.Learning, Title = "New approach", Body = "", CreatedAt = DateTime.UtcNow }
        };
        var md = _svc.BuildWeeklySummary(new DateOnly(2026,3,2), new DateOnly(2026,3,8), [], journal);
        Assert.Contains("### 🎓 Learnings", md);
    }

    [Fact]
    public void BuildWeeklySummary_NoJournalOfType_OmitsSection()
    {
        var md = _svc.BuildWeeklySummary(new DateOnly(2026,3,2), new DateOnly(2026,3,8), [], []);
        Assert.DoesNotContain("### 🏆", md);
        Assert.DoesNotContain("### ⚡", md);
        Assert.DoesNotContain("### 🎓", md);
    }

    [Fact]
    public void BuildReviewExport_ItemsOrderedByDateAscending()
    {
        var entries = new List<JournalEntry>
        {
            new() { Date = new DateOnly(2026, 3, 10), Type = JournalEntryType.Success, Title = "Later win", Body = "", CreatedAt = DateTime.UtcNow },
            new() { Date = new DateOnly(2026, 1,  5), Type = JournalEntryType.Success, Title = "Earlier win", Body = "", CreatedAt = DateTime.UtcNow },
        };
        var md = _svc.BuildReviewExport(new DateOnly(2026,1,1), new DateOnly(2026,12,31), entries);
        var earlierIdx = md.IndexOf("Earlier win", StringComparison.Ordinal);
        var laterIdx   = md.IndexOf("Later win",   StringComparison.Ordinal);
        Assert.True(earlierIdx < laterIdx, "Earlier date should appear before later date in review export.");
    }

    [Fact]
    public void BuildReviewExport_EmptyList_NoSections()
    {
        var md = _svc.BuildReviewExport(new DateOnly(2026,1,1), new DateOnly(2026,12,31), []);
        Assert.DoesNotContain("🏆", md);
        Assert.DoesNotContain("⚡", md);
        Assert.DoesNotContain("🎓", md);
    }

    [Fact]
    public void BuildDailyNote_FormatHours_ZeroHours()
    {
        // entry with zero duration produces "0h 0m"
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [], [], 0);
        Assert.Contains("0h 0m", md); // total time line
    }

    [Fact]
    public void BuildDailyNote_TotalHours_CalculatedFromEntryDurations()
    {
        var entry = new TimeEntry
        {
            StartTime = new DateTime(2026, 3, 9, 9, 0, 0, DateTimeKind.Utc),
            EndTime   = new DateTime(2026, 3, 9, 10, 30, 0, DateTimeKind.Utc),
        };
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [entry], [], 0);
        Assert.Contains("1h 30m", md);
    }
}
