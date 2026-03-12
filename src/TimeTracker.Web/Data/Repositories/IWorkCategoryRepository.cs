using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories;

public interface IWorkCategoryRepository
{
    Task<WorkCategory?> GetByIdAsync(int id);
    Task<List<WorkCategory>> GetAllAsync();
    Task<WorkCategory> AddAsync(WorkCategory category);
    Task UpdateAsync(WorkCategory category);
    Task DeleteAsync(int id);
}
