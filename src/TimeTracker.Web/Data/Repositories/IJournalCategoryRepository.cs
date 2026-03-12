using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories;

public interface IJournalCategoryRepository
{
    Task<JournalCategory?> GetByIdAsync(int id);
    Task<List<JournalCategory>> GetAllAsync();
    Task<JournalCategory> AddAsync(JournalCategory category);
    Task UpdateAsync(JournalCategory category);
    Task DeleteAsync(int id);
}
