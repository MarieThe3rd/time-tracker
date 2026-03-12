using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Features.Reports.AiUsage;

namespace TimeTracker.Web.Features.Reports;

public class MarkdownExportService
{
    public string BuildDailyNote(
        DateOnly date,
        List<TimeEntry> entries,
        List<JournalEntry> journalEntries,
        double avgProductivity)
    {
        var totalHours = entries
            .Where(e => e.Duration.HasValue)
            .Sum(e => e.Duration!.Value.TotalHours);
        var aiEntries = entries
            .Where(e => e.AiUsed)
            .OrderBy(e => e.StartTime)
            .ToList();
        var totalAiTimeSavedMinutes = aiEntries.Sum(e => e.AiTimeSavedMinutes ?? 0);

        var sb = new System.Text.StringBuilder();

        // YAML frontmatter
        sb.AppendLine("---");
        sb.AppendLine($"date: {date:yyyy-MM-dd}");
        sb.AppendLine($"total_hours: {totalHours:F2}");
        sb.AppendLine($"productivity_score: {(avgProductivity > 0 ? avgProductivity.ToString("F1") : "n/a")}");
        sb.AppendLine($"ai_usage_count: {aiEntries.Count}");
        sb.AppendLine($"ai_time_saved_minutes: {totalAiTimeSavedMinutes}");
        sb.AppendLine("tags: [time-tracker]");
        sb.AppendLine("---");
        sb.AppendLine();

        sb.AppendLine("## ⏱ Time Tracker");
        sb.AppendLine();

        // Summary
        sb.AppendLine($"**Date:** {date:dddd, MMMM d, yyyy}  ");
        sb.AppendLine($"**Total time:** {FormatHours(totalHours)}  ");
        if (avgProductivity > 0)
            sb.AppendLine($"**Avg productivity:** {avgProductivity:F1}/5  ");
        if (aiEntries.Count > 0)
        {
            sb.AppendLine($"**AI-assisted entries:** {aiEntries.Count}  ");
            sb.AppendLine($"**AI time saved:** {FormatMinutes(totalAiTimeSavedMinutes)}  ");
        }
        sb.AppendLine();

        // Time entries table
        if (entries.Count > 0)
        {
            sb.AppendLine("### Time Entries");
            sb.AppendLine();
            sb.AppendLine("| Time | Category | Description | Duration | ★ |");
            sb.AppendLine("|------|----------|-------------|----------|---|");
            foreach (var e in entries.OrderBy(e => e.StartTime))
            {
                var time = e.StartTime.ToLocalTime().ToString("HH:mm");
                var cat = e.WorkCategory?.Name ?? "—";
                var desc = e.Description ?? "—";
                var dur = e.Duration.HasValue ? FormatHours(e.Duration.Value.TotalHours) : "—";
                var rating = e.ProductivityRating.HasValue ? new string('★', e.ProductivityRating.Value) : "—";
                sb.AppendLine($"| {time} | {cat} | {desc} | {dur} | {rating} |");
            }
            sb.AppendLine();
        }

        if (aiEntries.Count > 0)
        {
            sb.AppendLine("### AI Usage");
            sb.AppendLine();
            sb.AppendLine("| Time | Description | Time Saved | Value Added | Notes |");
            sb.AppendLine("|------|-------------|------------|-------------|-------|");
            foreach (var entry in aiEntries)
            {
                var time = entry.StartTime.ToLocalTime().ToString("HH:mm");
                var description = EscapePipe(entry.Description ?? "—");
                var timeSaved = FormatMinutes(entry.AiTimeSavedMinutes ?? 0);
                var valueAdded = EscapePipe(string.IsNullOrWhiteSpace(entry.ValueAdded) ? "—" : entry.ValueAdded);
                var notes = EscapePipe(string.IsNullOrWhiteSpace(entry.AiNotes) ? "—" : entry.AiNotes);
                sb.AppendLine($"| {time} | {description} | {timeSaved} | {valueAdded} | {notes} |");
            }
            sb.AppendLine();
        }

        // Journal entries
        if (journalEntries.Count > 0)
        {
            sb.AppendLine("### Journal");
            sb.AppendLine();
            foreach (var j in journalEntries)
            {
                var icon = j.JournalTypeId switch
                {
                    1 => "⚡",
                    2 => "🎓",
                    3 => "🏆",
                    _ => "•"
                };
                var typeName = j.JournalType?.Name ?? j.JournalTypeId.ToString();
                sb.AppendLine($"#### {icon} {typeName}: {j.Title}");
                if (!string.IsNullOrWhiteSpace(j.Body))
                    sb.AppendLine(j.Body);
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    public string BuildWeeklySummary(
        DateOnly from,
        DateOnly to,
        List<TimeEntry> entries,
        List<JournalEntry> journalEntries)
    {
        var totalHours = entries
            .Where(e => e.Duration.HasValue)
            .Sum(e => e.Duration!.Value.TotalHours);
        var aiEntries = entries
            .Where(e => e.AiUsed)
            .OrderBy(e => e.StartTime)
            .ToList();
        var totalAiTimeSavedMinutes = aiEntries.Sum(e => e.AiTimeSavedMinutes ?? 0);

        var sb = new System.Text.StringBuilder();

        sb.AppendLine("---");
        sb.AppendLine($"date: {to:yyyy-MM-dd}");
        sb.AppendLine($"period: \"{from:MMM d} – {to:MMM d, yyyy}\"");
        sb.AppendLine($"total_hours: {totalHours:F2}");
        sb.AppendLine("tags: [time-tracker, weekly-summary]");
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"## 📊 Weekly Summary: {from:MMM d} – {to:MMM d, yyyy}");
        sb.AppendLine();
        sb.AppendLine($"**Total time logged:** {FormatHours(totalHours)}");
        if (aiEntries.Count > 0)
        {
            sb.AppendLine($"**AI-assisted entries:** {aiEntries.Count}");
            sb.AppendLine($"**AI time saved:** {FormatMinutes(totalAiTimeSavedMinutes)}");
        }
        sb.AppendLine();

        // Category breakdown
        var byCat = entries
            .Where(e => e.Duration.HasValue && e.WorkCategory != null)
            .GroupBy(e => e.WorkCategory!.Name)
            .Select(g => (Name: g.Key, Hours: g.Sum(e => e.Duration!.Value.TotalHours)))
            .OrderByDescending(x => x.Hours)
            .ToList();

        if (byCat.Count > 0)
        {
            sb.AppendLine("### Time by Category");
            sb.AppendLine();
            sb.AppendLine("| Category | Hours |");
            sb.AppendLine("|----------|-------|");
            foreach (var (name, hours) in byCat)
                sb.AppendLine($"| {name} | {FormatHours(hours)} |");
            sb.AppendLine();
        }

        AppendAiUsageSummary(sb, aiEntries);

        // Wins
        var wins = journalEntries.Where(j => j.JournalTypeId == 3).ToList();
        if (wins.Count > 0)
        {
            sb.AppendLine("### 🏆 Wins");
            sb.AppendLine();
            foreach (var w in wins)
            {
                sb.AppendLine($"- **{w.Title}**");
                if (!string.IsNullOrWhiteSpace(w.Body))
                    sb.AppendLine($"  {w.Body}");
            }
            sb.AppendLine();
        }

        // Challenges
        var challenges = journalEntries.Where(j => j.JournalTypeId == 1).ToList();
        if (challenges.Count > 0)
        {
            sb.AppendLine("### ⚡ Challenges");
            sb.AppendLine();
            foreach (var c in challenges)
            {
                sb.AppendLine($"- **{c.Title}**");
                if (!string.IsNullOrWhiteSpace(c.Body))
                    sb.AppendLine($"  {c.Body}");
            }
            sb.AppendLine();
        }

        // Learnings
        var learnings = journalEntries.Where(j => j.JournalTypeId == 2).ToList();
        if (learnings.Count > 0)
        {
            sb.AppendLine("### 🎓 Learnings");
            sb.AppendLine();
            foreach (var l in learnings)
            {
                sb.AppendLine($"- **{l.Title}**");
                if (!string.IsNullOrWhiteSpace(l.Body))
                    sb.AppendLine($"  {l.Body}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public string BuildReviewExport(
        DateOnly from,
        DateOnly to,
        List<JournalEntry> journalEntries,
        List<TimeEntry> entries)
    {
        var aiEntries = entries
            .Where(e => e.AiUsed)
            .OrderBy(e => e.StartTime)
            .ToList();

        var sb = new System.Text.StringBuilder();

        sb.AppendLine("---");
        sb.AppendLine($"period: \"{from:MMM d} – {to:MMM d, yyyy}\"");
        sb.AppendLine("tags: [time-tracker, review]");
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"## 📋 Review Export: {from:MMM d} – {to:MMM d, yyyy}");
        sb.AppendLine();

        void WriteSection(string heading, int journalTypeId, string icon)
        {
            var items = journalEntries.Where(j => j.JournalTypeId == journalTypeId)
                                      .OrderBy(j => j.Date)
                                      .ToList();
            if (items.Count == 0) return;
            sb.AppendLine($"### {icon} {heading}");
            sb.AppendLine();
            foreach (var item in items)
            {
                sb.AppendLine($"**{item.Date:MMM d, yyyy} — {item.Title}**");
                if (!string.IsNullOrWhiteSpace(item.Body))
                    sb.AppendLine(item.Body);
                sb.AppendLine();
            }
        }

        WriteSection("Successes & Wins", 3, "🏆");
        WriteSection("Challenges", 1, "⚡");
        WriteSection("Learnings", 2, "🎓");
        AppendAiUsageSummary(sb, aiEntries);

        return sb.ToString();
    }

    /// <summary>
    /// Builds a markdown AI usage weekly report with YAML frontmatter suitable for an Obsidian vault.
    /// Emits a single-row placeholder when <paramref name="weeks"/> is empty.
    /// </summary>
    public string BuildAiUsageWeeklyReport(DateOnly from, DateOnly to, List<AiUsageWeeklyItem> weeks)
    {
        var sb = new System.Text.StringBuilder();

        // YAML frontmatter — consistent style with BuildWeeklySummary / BuildReviewExport
        sb.AppendLine("---");
        sb.AppendLine($"period: \"{from:yyyy-MM-dd} to {to:yyyy-MM-dd}\"");
        sb.AppendLine("tags: [time-tracker, ai-usage-report]");
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"# AI Usage Report for {from:yyyy-MM-dd} to {to:yyyy-MM-dd}");
        sb.AppendLine();

        sb.AppendLine("| Week Start | Week End | AI-Assisted Work Done | Value Added | Time Saved (minutes) | Notes |");
        sb.AppendLine("|------|------|-----------------------|-------------|----------------------|-------|");

        if (weeks.Count == 0)
        {
            sb.AppendLine("| — | — | 0 | — | 0 | No AI activity recorded. |");
        }
        else
        {
            foreach (var w in weeks)
            {
                var valueAdded = string.IsNullOrWhiteSpace(w.ValueAdded) ? "—" : EscapePipe(w.ValueAdded);
                var notes = string.IsNullOrWhiteSpace(w.Notes) ? "—" : EscapePipe(w.Notes);
                sb.AppendLine($"| {w.WeekStart:yyyy-MM-dd} | {w.WeekEnd:yyyy-MM-dd} | {w.AiTaskCount} | {valueAdded} | {w.TotalTimeSavedMinutes} | {notes} |");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Escapes pipe characters in user-supplied strings so they do not break markdown table columns.
    /// </summary>
    private static string EscapePipe(string s) => s.Replace("|", "\\|");

    private static void AppendAiUsageSummary(System.Text.StringBuilder sb, List<TimeEntry> aiEntries)
    {
        if (aiEntries.Count == 0)
        {
            return;
        }

        var totalAiTimeSavedMinutes = aiEntries.Sum(e => e.AiTimeSavedMinutes ?? 0);
        var weeklyBreakdown = aiEntries
            .GroupBy(e => GetWeekStart(DateOnly.FromDateTime(e.StartTime.ToLocalTime())))
            .OrderBy(g => g.Key)
            .Select(g => new AiUsageWeeklyItem
            {
                WeekStart = g.Key,
                WeekEnd = g.Key.AddDays(6),
                AiTaskCount = g.Count(),
                TotalTimeSavedMinutes = g.Sum(e => e.AiTimeSavedMinutes ?? 0),
                ValueAdded = string.Join("; ", g.Where(e => !string.IsNullOrWhiteSpace(e.ValueAdded)).Select(e => e.ValueAdded!)),
                Notes = string.Join("; ", g.Where(e => !string.IsNullOrWhiteSpace(e.AiNotes)).Select(e => e.AiNotes!))
            })
            .ToList();

        sb.AppendLine("### AI Usage Summary");
        sb.AppendLine();
        sb.AppendLine($"**AI-assisted entries:** {aiEntries.Count}");
        sb.AppendLine($"**AI time saved:** {FormatMinutes(totalAiTimeSavedMinutes)}");
        sb.AppendLine();
        sb.AppendLine("| Week | AI Entries | Time Saved | Value Added | Notes |");
        sb.AppendLine("|------|------------|------------|-------------|-------|");
        foreach (var week in weeklyBreakdown)
        {
            var valueAdded = EscapePipe(string.IsNullOrWhiteSpace(week.ValueAdded) ? "—" : week.ValueAdded);
            var notes = EscapePipe(string.IsNullOrWhiteSpace(week.Notes) ? "—" : week.Notes);
            sb.AppendLine($"| {week.WeekStart:yyyy-MM-dd} | {week.AiTaskCount} | {FormatMinutes(week.TotalTimeSavedMinutes)} | {valueAdded} | {notes} |");
        }
        sb.AppendLine();
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }

    private static string FormatMinutes(int minutes)
    {
        var span = TimeSpan.FromMinutes(minutes);
        return $"{(int)span.TotalHours}h {span.Minutes}m";
    }

    private static string FormatHours(double hours)
    {
        var h = (int)hours;
        var m = (int)Math.Round((hours - h) * 60);
        return $"{h}h {m}m";
    }
}
