using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Features.Reports;
using TimeTracker.Web.Features.Reports.AiUsage;

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
            EndTime = new DateTime(2026, 3, 9, 11, 0, 0, DateTimeKind.Utc),
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
    public void BuildDailyNote_WithAiEntries_RendersAiUsageSection()
    {
        var entry = new TimeEntry
        {
            StartTime = new DateTime(2026, 3, 9, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 3, 9, 10, 0, 0, DateTimeKind.Utc),
            Description = "Draft architecture note",
            AiUsed = true,
            AiTimeSavedMinutes = 25,
            ValueAdded = "Clearer draft",
            AiNotes = "Summarized options"
        };

        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [entry], [], 0);

        Assert.Contains("ai_usage_count: 1", md);
        Assert.Contains("### AI Usage", md);
        Assert.Contains("Draft architecture note", md);
        Assert.Contains("Clearer draft", md);
        Assert.Contains("Summarized options", md);
    }

    [Fact]
    public void BuildWeeklySummary_ContainsCorrectPeriod()
    {
        var from = new DateOnly(2026, 3, 2);
        var to = new DateOnly(2026, 3, 8);
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
        var md = _svc.BuildReviewExport(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), entries, []);
        Assert.Contains("🏆", md);
        Assert.Contains("⚡", md);
        Assert.Contains("🎓", md);
        Assert.Contains("Win", md);
        Assert.Contains("Hard bug", md);
        Assert.Contains("Learned X", md);
    }

    [Fact]
    public void BuildReviewExport_WithAiEntries_ShowsAiUsageSummarySection()
    {
        var journal = new List<JournalEntry>
        {
            new() { Date = new DateOnly(2026, 1, 3), Type = JournalEntryType.Success, Title = "Shipped UI", Body = "", CreatedAt = DateTime.UtcNow }
        };
        var entries = new List<TimeEntry>
        {
            new()
            {
                StartTime = new DateTime(2026, 1, 5, 9, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc),
                AiUsed = true,
                AiTimeSavedMinutes = 15,
                ValueAdded = "Faster review notes",
                AiNotes = "Generated summary bullets"
            }
        };

        var md = _svc.BuildReviewExport(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), journal, entries);

        Assert.Contains("### AI Usage Summary", md);
        Assert.Contains("**AI-assisted entries:** 1", md);
        Assert.Contains("**AI time saved:** 0h 15m", md);
        Assert.Contains("Faster review notes", md);
        Assert.Contains("Generated summary bullets", md);
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
            EndTime = new DateTime(2026, 3, 9, 11, 0, 0, DateTimeKind.Utc),
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
            EndTime = new DateTime(2026, 3, 9, 11, 0, 0, DateTimeKind.Utc),
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
        var md = _svc.BuildWeeklySummary(new DateOnly(2026, 3, 2), new DateOnly(2026, 3, 8), entries, []);
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
        var md = _svc.BuildWeeklySummary(new DateOnly(2026, 3, 2), new DateOnly(2026, 3, 8), [], journal);
        Assert.Contains("### 🏆 Wins", md);
        Assert.Contains("Big win", md);
    }

    [Fact]
    public void BuildWeeklySummary_WithAiEntries_ShowsAiUsageSummarySection()
    {
        var entries = new List<TimeEntry>
        {
            new()
            {
                StartTime = new DateTime(2026, 3, 2, 9, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 3, 2, 10, 0, 0, DateTimeKind.Utc),
                AiUsed = true,
                AiTimeSavedMinutes = 10,
                ValueAdded = "Reduced boilerplate",
                AiNotes = "Generated handler skeleton"
            }
        };

        var md = _svc.BuildWeeklySummary(new DateOnly(2026, 3, 2), new DateOnly(2026, 3, 8), entries, []);

        Assert.Contains("**AI-assisted entries:** 1", md);
        Assert.Contains("### AI Usage Summary", md);
        Assert.Contains("Reduced boilerplate", md);
        Assert.Contains("Generated handler skeleton", md);
    }

    [Fact]
    public void BuildWeeklySummary_WithChallenges_ShowsChallengesSection()
    {
        var journal = new List<JournalEntry>
        {
            new() { Date = new DateOnly(2026,3,5), Type = JournalEntryType.Challenge, Title = "Blocked", Body = "", CreatedAt = DateTime.UtcNow }
        };
        var md = _svc.BuildWeeklySummary(new DateOnly(2026, 3, 2), new DateOnly(2026, 3, 8), [], journal);
        Assert.Contains("### ⚡ Challenges", md);
    }

    [Fact]
    public void BuildWeeklySummary_WithLearnings_ShowsLearningsSection()
    {
        var journal = new List<JournalEntry>
        {
            new() { Date = new DateOnly(2026,3,5), Type = JournalEntryType.Learning, Title = "New approach", Body = "", CreatedAt = DateTime.UtcNow }
        };
        var md = _svc.BuildWeeklySummary(new DateOnly(2026, 3, 2), new DateOnly(2026, 3, 8), [], journal);
        Assert.Contains("### 🎓 Learnings", md);
    }

    [Fact]
    public void BuildWeeklySummary_NoJournalOfType_OmitsSection()
    {
        var md = _svc.BuildWeeklySummary(new DateOnly(2026, 3, 2), new DateOnly(2026, 3, 8), [], []);
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
        var md = _svc.BuildReviewExport(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), entries, []);
        var earlierIdx = md.IndexOf("Earlier win", StringComparison.Ordinal);
        var laterIdx = md.IndexOf("Later win", StringComparison.Ordinal);
        Assert.True(earlierIdx < laterIdx, "Earlier date should appear before later date in review export.");
    }

    [Fact]
    public void BuildReviewExport_EmptyList_NoSections()
    {
        var md = _svc.BuildReviewExport(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), [], []);
        Assert.DoesNotContain("🏆", md);
        Assert.DoesNotContain("⚡", md);
        Assert.DoesNotContain("🎓", md);
        Assert.DoesNotContain("### AI Usage Summary", md);
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
            EndTime = new DateTime(2026, 3, 9, 10, 30, 0, DateTimeKind.Utc),
        };
        var md = _svc.BuildDailyNote(new DateOnly(2026, 3, 9), [entry], [], 0);
        Assert.Contains("1h 30m", md);
    }

    // ── BuildAiUsageWeeklyReport tests ──────────────────────────────────────────

    private static List<AiUsageWeeklyItem> OneWeekList() =>
    [
        new AiUsageWeeklyItem
        {
            WeekStart             = new DateOnly(2026, 1, 5),
            WeekEnd               = new DateOnly(2026, 1, 11),
            AiTaskCount           = 3,
            TotalTimeSavedMinutes = 45,
            ValueAdded            = "Faster code review",
            Notes                 = "Used Copilot for PR"
        }
    ];

    [Fact]
    public void BuildAiUsageWeeklyReport_FrontmatterAndHeading_Present()
    {
        var from = new DateOnly(2026, 1, 1);
        var to = new DateOnly(2026, 1, 31);
        var md = _svc.BuildAiUsageWeeklyReport(from, to, OneWeekList());

        Assert.Contains("---", md);
        Assert.Contains("tags: [time-tracker, ai-usage-report]", md);
        Assert.Contains("# AI Usage Report for", md);
    }

    [Fact]
    public void BuildAiUsageWeeklyReport_TableHeaders_Present()
    {
        var md = _svc.BuildAiUsageWeeklyReport(
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), OneWeekList());

        Assert.Contains("| Week Start |", md);
        Assert.Contains("| Week End |", md);
        Assert.Contains("| AI-Assisted Work Done |", md);
        Assert.Contains("| Value Added |", md);
        Assert.Contains("| Time Saved (minutes) |", md);
        Assert.Contains("| Notes |", md);
    }

    [Fact]
    public void BuildAiUsageWeeklyReport_SingleWeekRow_ContainsExpectedValues()
    {
        var md = _svc.BuildAiUsageWeeklyReport(
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), OneWeekList());

        Assert.Contains("2026-01-05", md);   // WeekStart date string
        Assert.Contains("2026-01-11", md);   // WeekEnd date string
        Assert.Contains("3", md);            // AiTaskCount
        Assert.Contains("45", md);           // TotalTimeSavedMinutes
        Assert.Contains("Faster code review", md);
        Assert.Contains("Used Copilot for PR", md);
    }

    [Fact]
    public void BuildAiUsageWeeklyReport_EmptyWeeks_ShowsPlaceholderRow()
    {
        var md = _svc.BuildAiUsageWeeklyReport(
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), []);

        Assert.Contains("No AI activity recorded.", md);
    }

    [Fact]
    public void BuildAiUsageWeeklyReport_MultipleWeeks_BothDatesAppear()
    {
        var weeks = new List<AiUsageWeeklyItem>
        {
            new() { WeekStart = new DateOnly(2026, 1, 5),  WeekEnd = new DateOnly(2026, 1, 11), AiTaskCount = 1, TotalTimeSavedMinutes = 10 },
            new() { WeekStart = new DateOnly(2026, 1, 12), WeekEnd = new DateOnly(2026, 1, 18), AiTaskCount = 2, TotalTimeSavedMinutes = 20 },
        };
        var md = _svc.BuildAiUsageWeeklyReport(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), weeks);

        Assert.Contains("2026-01-05", md);
        Assert.Contains("2026-01-12", md);
        var firstIdx = md.IndexOf("2026-01-05", StringComparison.Ordinal);
        var secondIdx = md.IndexOf("2026-01-12", StringComparison.Ordinal);
        Assert.True(firstIdx < secondIdx, "First week row should appear before second week row.");
    }

    [Fact]
    public void BuildAiUsageWeeklyReport_EmptyValueAddedAndNotes_RenderAsDash()
    {
        var weeks = new List<AiUsageWeeklyItem>
        {
            new()
            {
                WeekStart             = new DateOnly(2026, 1, 5),
                WeekEnd               = new DateOnly(2026, 1, 11),
                AiTaskCount           = 1,
                TotalTimeSavedMinutes = 5,
                ValueAdded            = string.Empty,
                Notes                 = string.Empty
            }
        };
        var md = _svc.BuildAiUsageWeeklyReport(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), weeks);

        // The data row should render "—" for the empty ValueAdded and Notes columns.
        // We verify by checking that the row contains "| — |" at least once.
        var lines = md.Split('\n');
        var dataRow = lines.FirstOrDefault(l =>
            l.Contains("2026-01-05") && l.StartsWith("|"));
        Assert.NotNull(dataRow);
        Assert.Contains("| — |", dataRow);
    }

    [Fact]
    public void BuildAiUsageWeeklyReport_PipeInValueAdded_IsEscaped()
    {
        var weeks = new List<AiUsageWeeklyItem>
        {
            new()
            {
                WeekStart             = new DateOnly(2026, 1, 5),
                WeekEnd               = new DateOnly(2026, 1, 11),
                AiTaskCount           = 2,
                TotalTimeSavedMinutes = 30,
                ValueAdded            = "Used AI for search | refactoring",
                Notes                 = "PR review | code gen"
            }
        };
        var md = _svc.BuildAiUsageWeeklyReport(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), weeks);

        // Pipe characters in user values must be escaped so the table column count stays correct.
        Assert.Contains(@"Used AI for search \| refactoring", md);
        Assert.Contains(@"PR review \| code gen", md);
        // The raw (unescaped) pipe must NOT appear inside the data row.
        var lines = md.Split('\n');
        var dataRow = lines.FirstOrDefault(l => l.Contains("2026-01-05") && l.StartsWith("|"));
        Assert.NotNull(dataRow);
        Assert.DoesNotContain("search | refactoring", dataRow);
    }
}
