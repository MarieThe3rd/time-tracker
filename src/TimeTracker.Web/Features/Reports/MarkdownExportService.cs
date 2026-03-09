using TimeTracker.Web.Data.Models;

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

        var sb = new System.Text.StringBuilder();

        // YAML frontmatter
        sb.AppendLine("---");
        sb.AppendLine($"date: {date:yyyy-MM-dd}");
        sb.AppendLine($"total_hours: {totalHours:F2}");
        sb.AppendLine($"productivity_score: {(avgProductivity > 0 ? avgProductivity.ToString("F1") : "n/a")}");
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

        // Journal entries
        if (journalEntries.Count > 0)
        {
            sb.AppendLine("### Journal");
            sb.AppendLine();
            foreach (var j in journalEntries)
            {
                var icon = j.Type switch
                {
                    JournalEntryType.Challenge => "⚡",
                    JournalEntryType.Learning  => "🎓",
                    JournalEntryType.Success   => "🏆",
                    _ => "•"
                };
                sb.AppendLine($"#### {icon} {j.Type}: {j.Title}");
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

        // Wins
        var wins = journalEntries.Where(j => j.Type == JournalEntryType.Success).ToList();
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
        var challenges = journalEntries.Where(j => j.Type == JournalEntryType.Challenge).ToList();
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
        var learnings = journalEntries.Where(j => j.Type == JournalEntryType.Learning).ToList();
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
        List<JournalEntry> journalEntries)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("---");
        sb.AppendLine($"period: \"{from:MMM d} – {to:MMM d, yyyy}\"");
        sb.AppendLine("tags: [time-tracker, review]");
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"## 📋 Review Export: {from:MMM d} – {to:MMM d, yyyy}");
        sb.AppendLine();

        void WriteSection(string heading, JournalEntryType type, string icon)
        {
            var items = journalEntries.Where(j => j.Type == type)
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

        WriteSection("Successes & Wins", JournalEntryType.Success, "🏆");
        WriteSection("Challenges", JournalEntryType.Challenge, "⚡");
        WriteSection("Learnings", JournalEntryType.Learning, "🎓");

        return sb.ToString();
    }

    private static string FormatHours(double hours)
    {
        var h = (int)hours;
        var m = (int)Math.Round((hours - h) * 60);
        return $"{h}h {m}m";
    }
}
