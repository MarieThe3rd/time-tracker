namespace TimeTracker.Web.Data.Models;

public enum TaskItemStatus { NotStarted, InProgress, Done, Blocked }
public enum TaskItemPriority { Low, Medium, High, Critical }

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.NotStarted;
    public TaskItemPriority Priority { get; set; } = TaskItemPriority.Medium;
    public DateOnly? DueDate { get; set; }
    public string? AssignedBy { get; set; }
    public string? DeliverableTo { get; set; }
    public int? WorkCategoryId { get; set; }
    public WorkCategory? WorkCategory { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
