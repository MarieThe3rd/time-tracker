using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Timer;

public class StopTimerHandler(ITimeEntryRepository timeEntryRepo, RunningTimerService timerService)
{
    public async Task<TimeEntry> HandleAsync()
    {
        var (startedAt, categoryId, description) = timerService.Stop();

        var entry = new TimeEntry
        {
            StartTime = startedAt,
            EndTime = DateTime.UtcNow,
            WorkCategoryId = categoryId,
            Description = description
        };

        return await timeEntryRepo.AddAsync(entry);
    }
}
