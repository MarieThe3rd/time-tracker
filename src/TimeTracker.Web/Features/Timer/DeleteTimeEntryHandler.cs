using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Timer;

public class DeleteTimeEntryHandler(ITimeEntryRepository timeEntryRepo)
{
    public Task HandleAsync(int id) => timeEntryRepo.DeleteAsync(id);
}
