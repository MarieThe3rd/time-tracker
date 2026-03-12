using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Reminders;

public class SnoozeReminderHandler(IReminderRepository reminderRepo)
{
    public async Task HandleAsync(int id, DateTime newRemindOn)
    {
        if (newRemindOn == default)
            throw new ArgumentException("NewRemindOn must be set.", nameof(newRemindOn));

        var reminder = await reminderRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Reminder with Id {id} was not found.");

        reminder.RemindOn = newRemindOn;
        reminder.Status = ReminderStatus.Active;

        await reminderRepo.UpdateAsync(reminder);
    }
}
