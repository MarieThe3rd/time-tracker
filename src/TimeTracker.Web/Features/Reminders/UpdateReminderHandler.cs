using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Reminders;

public record UpdateReminderInput(
    int Id,
    string Title,
    DateTime RemindOn,
    ReminderRepeat Repeat,
    ReminderStatus Status,
    string? Notes = null);

public class UpdateReminderHandler(IReminderRepository reminderRepo)
{
    public async Task HandleAsync(UpdateReminderInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            throw new ArgumentException("Title cannot be empty.", nameof(input));

        var reminder = await reminderRepo.GetByIdAsync(input.Id)
            ?? throw new KeyNotFoundException($"Reminder with Id {input.Id} was not found.");

        reminder.Title = input.Title.Trim();
        reminder.Notes = input.Notes;
        reminder.RemindOn = input.RemindOn;
        reminder.Repeat = input.Repeat;
        reminder.Status = input.Status;

        await reminderRepo.UpdateAsync(reminder);
    }
}
