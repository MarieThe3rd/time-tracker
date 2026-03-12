using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Reminders;

public class DeleteReminderHandler(IReminderRepository reminderRepo)
{
    public async Task HandleAsync(int id)
        => await reminderRepo.DeleteAsync(id);
}
