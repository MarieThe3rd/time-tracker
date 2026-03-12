using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Tasks;

namespace TimeTracker.Tests.Features.Tasks;

public class UpdateTaskHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static (AddTaskHandler add, UpdateTaskHandler update) CreateHandlers(AppDbContext db)
    {
        var repo = new SqlTaskItemRepository(db);
        return (new AddTaskHandler(repo), new UpdateTaskHandler(repo));
    }

    [Fact]
    public async Task HandleAsync_UpdatesTitle()
    {
        using var db = CreateDb();
        var (add, update) = CreateHandlers(db);
        var task = await add.HandleAsync(new AddTaskInput("Original"));

        await update.HandleAsync(new UpdateTaskInput(task.Id, "Updated", null,
            TaskItemStatus.InProgress, TaskItemPriority.High, null, null, null, null, null));

        var saved = await db.TaskItems.FindAsync(task.Id);
        Assert.Equal("Updated", saved!.Title);
    }

    [Fact]
    public async Task HandleAsync_SetsUpdatedAt()
    {
        using var db = CreateDb();
        var (add, update) = CreateHandlers(db);
        var task = await add.HandleAsync(new AddTaskInput("Task"));
        var originalUpdatedAt = task.UpdatedAt;

        await Task.Delay(10);
        await update.HandleAsync(new UpdateTaskInput(task.Id, "Modified", null,
            TaskItemStatus.Done, TaskItemPriority.Low, null, null, null, null, null));

        var saved = await db.TaskItems.FindAsync(task.Id);
        Assert.True(saved!.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb();
        var (_, update) = CreateHandlers(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            update.HandleAsync(new UpdateTaskInput(9999, "Title", null,
                TaskItemStatus.NotStarted, TaskItemPriority.Medium, null, null, null, null, null)));
    }

    [Fact]
    public async Task HandleAsync_EmptyTitle_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var (add, update) = CreateHandlers(db);
        var task = await add.HandleAsync(new AddTaskInput("Task"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            update.HandleAsync(new UpdateTaskInput(task.Id, "", null,
                TaskItemStatus.NotStarted, TaskItemPriority.Medium, null, null, null, null, null)));
    }

    [Fact]
    public async Task HandleAsync_UpdatesStatus()
    {
        using var db = CreateDb();
        var (add, update) = CreateHandlers(db);
        var task = await add.HandleAsync(new AddTaskInput("Task"));

        await update.HandleAsync(new UpdateTaskInput(task.Id, "Task", null,
            TaskItemStatus.Done, TaskItemPriority.Medium, null, null, null, null, null));

        var saved = await db.TaskItems.FindAsync(task.Id);
        Assert.Equal(TaskItemStatus.Done, saved!.Status);
    }
}
