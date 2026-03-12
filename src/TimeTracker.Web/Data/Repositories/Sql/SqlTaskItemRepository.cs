using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories.Sql;

public class SqlTaskItemRepository(AppDbContext db) : ITaskItemRepository
{
    public async Task<TaskItem?> GetByIdAsync(int id)
        => await db.TaskItems.Include(t => t.WorkCategory).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<List<TaskItem>> GetFilteredAsync(
        TaskItemStatus? status = null,
        TaskItemPriority? priority = null,
        DateOnly? dueBefore = null,
        string? deliverableTo = null,
        int? workCategoryId = null,
        bool includeCategory = false)
    {
        var query = db.TaskItems.AsQueryable();

        if (includeCategory)
            query = query.Include(t => t.WorkCategory);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (dueBefore.HasValue)
            query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value <= dueBefore.Value);

        if (!string.IsNullOrWhiteSpace(deliverableTo))
            query = query.Where(t => t.DeliverableTo != null && 
                t.DeliverableTo.ToLower() == deliverableTo.ToLower());

        if (workCategoryId.HasValue)
            query = query.Where(t => t.WorkCategoryId == workCategoryId.Value);

        return await query.OrderBy(t => t.DueDate).ThenBy(t => t.Priority).ToListAsync();
    }

    public async Task<TaskItem> AddAsync(TaskItem task)
    {
        db.TaskItems.Add(task);
        await db.SaveChangesAsync();
        return task;
    }

    public async Task UpdateAsync(TaskItem task)
    {
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var task = await db.TaskItems.FindAsync(id);
        if (task is not null)
        {
            db.TaskItems.Remove(task);
            await db.SaveChangesAsync();
        }
    }
}
