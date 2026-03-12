using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Timer;

public class GetTodayEntriesHandler(ITimeEntryRepository timeEntryRepo)
{
    public Task<List<TimeEntry>> HandleAsync() =>
        timeEntryRepo.GetTodayAsync(includeCategory: true);
}
