using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Dashboard;

public record CategoryStat(string Name, string Color, double TotalHours);

public record AiDashboardSummary(
    int TodayAssistedEntries,
    int TodayTimeSavedMinutes,
    int WeekAssistedEntries,
    int WeekTimeSavedMinutes);

public record DashboardData(
    TimeSpan TotalToday,
    double AvgProductivity,
    List<CategoryStat> CategoryStats,
    List<TimeEntry> RecentEntries,
    List<JournalEntry> RecentJournal,
    AiDashboardSummary AiSummary);

public class DashboardHandler(ITimeEntryRepository timeEntryRepo, IJournalEntryRepository journalRepo)
{
    public async Task<DashboardData> HandleAsync()
    {
        var todayLocal = DateOnly.FromDateTime(DateTime.Now);
        var todayEndUtc = todayLocal.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Local).ToUniversalTime();
        var weekStart = GetWeekStart(DateOnly.FromDateTime(DateTime.Today))
            .ToDateTime(TimeOnly.MinValue)
            .ToUniversalTime();

        var allTodayEntries = await timeEntryRepo.GetTodayAsync(includeCategory: true);
        var todayEntries = allTodayEntries.Where(e => e.EndTime != null).ToList();

        var weekEntries = await timeEntryRepo.GetByDateRangeAsync(weekStart, todayEndUtc);
        var weekAiEntries = weekEntries.Where(e => e.AiUsed).ToList();

        var total = todayEntries.Aggregate(TimeSpan.Zero,
            (acc, e) => acc + (e.Duration ?? TimeSpan.Zero));

        var rated = todayEntries.Where(e => e.ProductivityRating.HasValue).ToList();
        var avgProd = rated.Count > 0 ? rated.Average(e => e.ProductivityRating!.Value) : 0;

        var stats = todayEntries
            .Where(e => e.WorkCategory != null)
            .GroupBy(e => e.WorkCategory!)
            .Select(g => new CategoryStat(
                g.Key.Name,
                g.Key.Color,
                g.Sum(e => (e.Duration ?? TimeSpan.Zero).TotalHours)))
            .OrderByDescending(s => s.TotalHours)
            .ToList();

        var recent = todayEntries
            .OrderByDescending(e => e.StartTime)
            .Take(5)
            .ToList();

        var journal = await journalRepo.GetRecentAsync(3);

        var todayAiEntries = todayEntries.Where(e => e.AiUsed).ToList();
        var aiSummary = new AiDashboardSummary(
            todayAiEntries.Count,
            todayAiEntries.Sum(e => e.AiTimeSavedMinutes ?? 0),
            weekAiEntries.Count,
            weekAiEntries.Sum(e => e.AiTimeSavedMinutes ?? 0));

        return new DashboardData(total, avgProd, stats, recent, journal, aiSummary);
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }
}
