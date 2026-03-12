using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Dashboard;

public record CategoryStat(string Name, string Color, double TotalHours);

public record AiDashboardSummary(
    int AssistedEntries,
    int TimeSavedMinutes);

public record DashboardData(
    TimeSpan TotalTime,
    double AvgProductivity,
    int EntryCount,
    List<CategoryStat> CategoryStats,
    List<TimeEntry> RecentEntries,
    List<JournalEntry> RecentJournal,
    AiDashboardSummary AiSummary,
    int OverdueTaskCount,
    int UpcomingReminderCount);

public class DashboardHandler(
    ITimeEntryRepository timeEntryRepo,
    IJournalEntryRepository journalRepo,
    ITaskItemRepository taskRepo,
    IReminderRepository reminderRepo)
{
    public Task<DashboardData> HandleAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return HandleAsync(today, today);
    }

    public async Task<DashboardData> HandleAsync(DateOnly from, DateOnly to)
    {
        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).ToUniversalTime();
        var toUtc = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Local).ToUniversalTime();

        var entries = await timeEntryRepo.GetByDateRangeAsync(fromUtc, toUtc, includeCategory: true);

        var total = entries.Aggregate(TimeSpan.Zero,
            (acc, e) => acc + (e.Duration ?? TimeSpan.Zero));

        var rated = entries.Where(e => e.ProductivityRating.HasValue).ToList();
        var avgProd = rated.Count > 0 ? rated.Average(e => e.ProductivityRating!.Value) : 0;

        var stats = entries
            .Where(e => e.WorkCategory != null)
            .GroupBy(e => e.WorkCategory!)
            .Select(g => new CategoryStat(
                g.Key.Name,
                g.Key.Color,
                g.Sum(e => (e.Duration ?? TimeSpan.Zero).TotalHours)))
            .OrderByDescending(s => s.TotalHours)
            .ToList();

        var recent = entries
            .OrderByDescending(e => e.StartTime)
            .Take(5)
            .ToList();

        var journal = await journalRepo.GetRecentAsync(3);

        var aiEntries = entries.Where(e => e.AiUsed).ToList();
        var aiSummary = new AiDashboardSummary(
            aiEntries.Count,
            aiEntries.Sum(e => e.AiTimeSavedMinutes ?? 0));

        var overdueCount = await taskRepo.GetOverdueCountAsync();
        var upcomingCount = await reminderRepo.GetUpcomingCountAsync(TimeSpan.FromHours(24));

        return new DashboardData(total, avgProd, entries.Count, stats, recent, journal, aiSummary,
            overdueCount, upcomingCount);
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }
}
