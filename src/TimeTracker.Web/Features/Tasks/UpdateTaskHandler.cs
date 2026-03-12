using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Tasks;

public record UpdateTaskInput(
    int Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskItemPriority Priority,
    DateOnly? DueDate,
    string? AssignedBy,
    string? DeliverableTo,
    int? WorkCategoryId,
    string? Notes);

public class UpdateTaskHandler(ITaskItemRepository taskRepo)
{
    public async Task HandleAsync(UpdateTaskInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            throw new ArgumentException("Title cannot be empty.", nameof(input));

        var task = await taskRepo.GetByIdAsync(input.Id)
            ?? throw new KeyNotFoundException($"Task with Id {input.Id} was not found.");

        task.Title = input.Title.Trim();
        task.Description = input.Description;
        task.Status = input.Status;
        task.Priority = input.Priority;
        task.DueDate = input.DueDate;
        task.AssignedBy = input.AssignedBy;
        task.DeliverableTo = input.DeliverableTo;
        task.WorkCategoryId = input.WorkCategoryId;
        task.Notes = input.Notes;
        task.UpdatedAt = DateTime.UtcNow;

        await taskRepo.UpdateAsync(task);
    }
}
