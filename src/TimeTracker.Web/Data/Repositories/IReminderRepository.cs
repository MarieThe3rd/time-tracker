using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories;

public interface IReminderRepository
{
    Task<Reminder?> GetByIdAsync(int id);
    Task<List<Reminder>> GetActiveAsync();
    Task<List<Reminder>> GetAllAsync(bool includeDismissed = false);
    Task<int> GetUpcomingCountAsync(TimeSpan window);
    Task<Reminder> AddAsync(Reminder reminder);
    Task UpdateAsync(Reminder reminder);
    Task DeleteAsync(int id);
}
