using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Tasks;

public record AddTaskInput(
    string Title,
    string? Description = null,
    TaskItemPriority Priority = TaskItemPriority.Medium,
    DateOnly? DueDate = null,
    string? AssignedBy = null,
    string? DeliverableTo = null,
    int? WorkCategoryId = null,
    string? Notes = null);

public class AddTaskHandler(ITaskItemRepository taskRepo)
{
    public async Task<TaskItem> HandleAsync(AddTaskInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            throw new ArgumentException("Title cannot be empty.", nameof(input));

        var task = new TaskItem
        {
            Title = input.Title.Trim(),
            Description = input.Description,
            Status = TaskItemStatus.NotStarted,
            Priority = input.Priority,
            DueDate = input.DueDate,
            AssignedBy = input.AssignedBy,
            DeliverableTo = input.DeliverableTo,
            WorkCategoryId = input.WorkCategoryId,
            Notes = input.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await taskRepo.AddAsync(task);
    }
}
