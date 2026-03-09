using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Features.Timer;

public class GetTodayEntriesHandler(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<TimeEntry>> HandleAsync()
    {
        var todayLocal = DateOnly.FromDateTime(DateTime.Now);
        var startUtc = todayLocal.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).ToUniversalTime();
        var endUtc = todayLocal.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Local).ToUniversalTime();

        return await _db.TimeEntries
            .Include(e => e.WorkCategory)
            .Where(e => e.StartTime >= startUtc && e.StartTime <= endUtc)
            .OrderByDescending(e => e.StartTime)
            .ToListAsync();
    }
}
