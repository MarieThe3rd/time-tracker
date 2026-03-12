using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Reminders;

public class DismissReminderHandler(IReminderRepository reminderRepo)
{
    public async Task HandleAsync(int id)
    {
        var reminder = await reminderRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Reminder with Id {id} was not found.");

        reminder.Status = ReminderStatus.Dismissed;
        await reminderRepo.UpdateAsync(reminder);

        if (reminder.Repeat != ReminderRepeat.None)
        {
            var nextRemindOn = reminder.Repeat == ReminderRepeat.Daily
                ? reminder.RemindOn.AddDays(1)
                : reminder.RemindOn.AddDays(7);

            var next = new Reminder
            {
                Title = reminder.Title,
                Notes = reminder.Notes,
                RemindOn = nextRemindOn,
                Repeat = reminder.Repeat,
                Status = ReminderStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await reminderRepo.AddAsync(next);
        }
    }
}
