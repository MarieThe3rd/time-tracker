using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories.Sql;

public class SqlReminderRepository(AppDbContext db) : IReminderRepository
{
    public async Task<Reminder?> GetByIdAsync(int id)
        => await db.Reminders.FindAsync(id);

    public async Task<List<Reminder>> GetActiveAsync()
        => await db.Reminders
            .Where(r => r.Status == ReminderStatus.Active || r.Status == ReminderStatus.Snoozed)
            .OrderBy(r => r.RemindOn)
            .ToListAsync();

    public async Task<List<Reminder>> GetAllAsync(bool includeDismissed = false)
    {
        var query = db.Reminders.AsQueryable();
        if (!includeDismissed)
            query = query.Where(r => r.Status != ReminderStatus.Dismissed);
        return await query.OrderBy(r => r.RemindOn).ToListAsync();
    }

    public async Task<int> GetUpcomingCountAsync(TimeSpan window)
    {
        var now = DateTime.UtcNow;
        var cutoff = now + window;
        return await db.Reminders
            .CountAsync(r => (r.Status == ReminderStatus.Active || r.Status == ReminderStatus.Snoozed)
                          && r.RemindOn >= now
                          && r.RemindOn <= cutoff);
    }

    public async Task<Reminder> AddAsync(Reminder reminder)
    {
        db.Reminders.Add(reminder);
        await db.SaveChangesAsync();
        return reminder;
    }

    public async Task UpdateAsync(Reminder reminder)
    {
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var reminder = await db.Reminders.FindAsync(id);
        if (reminder is not null)
        {
            db.Reminders.Remove(reminder);
            await db.SaveChangesAsync();
        }
    }
}
