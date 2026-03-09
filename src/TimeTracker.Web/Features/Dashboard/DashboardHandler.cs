using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Features.Dashboard;

public record CategoryStat(string Name, string Color, double TotalHours);

public record DashboardData(
    TimeSpan TotalToday,
    double AvgProductivity,
    List<CategoryStat> CategoryStats,
    List<TimeEntry> RecentEntries,
    List<JournalEntry> RecentJournal);

public class DashboardHandler(AppDbContext db)
{
    public async Task<DashboardData> HandleAsync()
    {
        var todayStart = DateTime.Today.ToUniversalTime();
        var todayEnd = todayStart.AddDays(1);

        var todayEntries = await db.TimeEntries
            .Include(e => e.WorkCategory)
            .Where(e => e.StartTime >= todayStart && e.StartTime < todayEnd && e.EndTime != null)
            .ToListAsync();

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

        var journal = await db.JournalEntries
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.CreatedAt)
            .Take(3)
            .ToListAsync();

        return new DashboardData(total, avgProd, stats, recent, journal);
    }
}
