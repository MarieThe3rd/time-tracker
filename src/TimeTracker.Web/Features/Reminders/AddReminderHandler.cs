using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Reminders;

public record AddReminderInput(
    string Title,
    DateTime RemindOn,
    string? Notes = null,
    ReminderRepeat Repeat = ReminderRepeat.None);

public class AddReminderHandler(IReminderRepository reminderRepo)
{
    public async Task<Reminder> HandleAsync(AddReminderInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            throw new ArgumentException("Title cannot be empty.", nameof(input));

        if (input.RemindOn == default)
            throw new ArgumentException("RemindOn must be set.", nameof(input));

        var reminder = new Reminder
        {
            Title = input.Title.Trim(),
            Notes = input.Notes,
            RemindOn = input.RemindOn,
            Repeat = input.Repeat,
            Status = ReminderStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        return await reminderRepo.AddAsync(reminder);
    }
}
