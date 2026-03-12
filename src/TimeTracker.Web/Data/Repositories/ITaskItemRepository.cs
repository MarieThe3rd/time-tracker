using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories;

public interface ITaskItemRepository
{
    Task<TaskItem?> GetByIdAsync(int id);
    Task<List<TaskItem>> GetFilteredAsync(
        TaskItemStatus? status = null,
        TaskItemPriority? priority = null,
        DateOnly? dueBefore = null,
        string? deliverableTo = null,
        int? workCategoryId = null,
        bool includeCategory = false);
    Task<TaskItem> AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(int id);
}
