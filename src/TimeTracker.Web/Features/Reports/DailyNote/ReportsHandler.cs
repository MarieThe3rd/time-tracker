using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Features.Reports.DailyNote;

public record ReportRange(DateOnly From, DateOnly To);

public class ReportsHandler(AppDbContext db)
{
    public async Task<(List<TimeEntry> Entries, List<JournalEntry> Journal)> GetRangeDataAsync(ReportRange range)
    {
        var from = range.From.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        var to = range.To.ToDateTime(TimeOnly.MaxValue).ToUniversalTime();

        var entries = await db.TimeEntries
            .Include(e => e.WorkCategory)
            .Where(e => e.StartTime >= from && e.StartTime <= to && e.EndTime != null)
            .OrderBy(e => e.StartTime)
            .ToListAsync();

        var journal = await db.JournalEntries
            .Where(j => j.Date >= range.From && j.Date <= range.To)
            .OrderBy(j => j.Date)
            .ToListAsync();

        return (entries, journal);
    }

    public async Task<(string Path, bool Appended)> PushDailyNoteAsync(
        DateOnly date,
        string markdown,
        UserSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.VaultRootPath))
            throw new InvalidOperationException("Vault root path is not configured. Go to Settings.");

        var folder = Path.Combine(settings.VaultRootPath, settings.DailyNotesSubfolder);
        Directory.CreateDirectory(folder);

        var filePath = Path.Combine(folder, $"{date:yyyy-MM-dd}.md");
        bool appended = false;

        const string sectionHeading = "## ⏱ Time Tracker";

        if (File.Exists(filePath))
        {
            var existing = await File.ReadAllTextAsync(filePath);
            if (existing.Contains(sectionHeading))
            {
                // Replace existing section
                var idx = existing.IndexOf(sectionHeading, StringComparison.Ordinal);
                var before = existing[..idx];
                // Find next ## heading after our section, or end of file
                var nextSection = existing.IndexOf("\n## ", idx + sectionHeading.Length, StringComparison.Ordinal);
                var after = nextSection >= 0 ? existing[nextSection..] : string.Empty;
                // Extract just our section from the new markdown (strip frontmatter)
                var sectionOnly = ExtractSection(markdown, sectionHeading);
                await File.WriteAllTextAsync(filePath, before + sectionOnly + after);
            }
            else
            {
                // Append section only (no frontmatter) to existing file
                var sectionOnly = ExtractSection(markdown, sectionHeading);
                await File.AppendAllTextAsync(filePath, "\n" + sectionOnly);
            }
            appended = true;
        }
        else
        {
            await File.WriteAllTextAsync(filePath, markdown);
        }

        return (filePath, appended);
    }

    private static string ExtractSection(string markdown, string heading)
    {
        var idx = markdown.IndexOf(heading, StringComparison.Ordinal);
        return idx >= 0 ? markdown[idx..] : markdown;
    }
}
