using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Reminders;

public class ListRemindersHandler(IReminderRepository reminderRepo)
{
    public async Task<List<Reminder>> HandleAsync(bool activeOnly = true, bool includeDismissed = false)
    {
        if (activeOnly)
            return await reminderRepo.GetActiveAsync();

        return await reminderRepo.GetAllAsync(includeDismissed);
    }
}
