using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories;

public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(int id);
    Task<List<TimeEntry>> GetByDateRangeAsync(DateTime start, DateTime end, bool includeCategory = false);
    Task<List<TimeEntry>> GetTodayAsync(bool includeCategory = false);
    Task<List<TimeEntry>> GetRecentAsync(int count, bool includeCategory = false);
    Task<TimeEntry> AddAsync(TimeEntry entry);
    Task UpdateAsync(TimeEntry entry);
    Task DeleteAsync(int id);
}
