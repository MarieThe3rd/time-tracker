using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories;

public interface IJournalEntryRepository
{
    Task<JournalEntry?> GetByIdAsync(int id);
    Task<List<JournalEntry>> GetFilteredAsync(JournalEntryType? type = null, DateOnly? from = null, DateOnly? to = null, bool includeLinkedTimeEntry = false);
    Task<List<JournalEntry>> GetRecentAsync(int count);
    Task<JournalEntry> AddAsync(JournalEntry entry);
    Task UpdateAsync(JournalEntry entry);
    Task DeleteAsync(int id);
}
