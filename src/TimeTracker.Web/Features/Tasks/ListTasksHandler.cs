using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Tasks;

public record TaskFilter(
    TaskItemStatus? Status = null,
    TaskItemPriority? Priority = null,
    DateOnly? DueBefore = null,
    string? DeliverableTo = null,
    int? WorkCategoryId = null);

public class ListTasksHandler(ITaskItemRepository taskRepo)
{
    public async Task<List<TaskItem>> HandleAsync(TaskFilter? filter = null)
    {
        filter ??= new TaskFilter();
        return await taskRepo.GetFilteredAsync(
            status: filter.Status,
            priority: filter.Priority,
            dueBefore: filter.DueBefore,
            deliverableTo: filter.DeliverableTo,
            workCategoryId: filter.WorkCategoryId,
            includeCategory: true);
    }
}
