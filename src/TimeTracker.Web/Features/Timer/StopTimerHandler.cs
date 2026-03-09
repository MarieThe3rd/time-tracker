using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Features.Timer;

public class StopTimerHandler(AppDbContext db, RunningTimerService timerService)
{
    private readonly AppDbContext _db = db;
    private readonly RunningTimerService _timerService = timerService;

    public async Task<TimeEntry> HandleAsync()
    {
        var (startedAt, categoryId, description) = _timerService.Stop();

        var entry = new TimeEntry
        {
            StartTime = startedAt,
            EndTime = DateTime.UtcNow,
            WorkCategoryId = categoryId,
            Description = description
        };

        _db.TimeEntries.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }
}
