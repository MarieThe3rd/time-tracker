using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories;

public interface IJournalTypeRepository
{
    Task<JournalType?> GetByIdAsync(int id);
    Task<List<JournalType>> GetAllAsync();
    Task<JournalType> AddAsync(JournalType journalType);
    Task UpdateAsync(JournalType journalType);
    /// <summary>Deletes a non-system journal type. Throws InvalidOperationException for system types.</summary>
    Task DeleteAsync(int id);
}
