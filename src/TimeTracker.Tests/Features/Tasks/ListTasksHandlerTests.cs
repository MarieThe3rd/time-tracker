using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Tasks;

namespace TimeTracker.Tests.Features.Tasks;

public class ListTasksHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static (AddTaskHandler add, ListTasksHandler list) CreateHandlers(AppDbContext db)
    {
        var repo = new SqlTaskItemRepository(db);
        return (new AddTaskHandler(repo), new ListTasksHandler(repo));
    }

    [Fact]
    public async Task HandleAsync_NoFilter_ReturnsAllTasks()
    {
        using var db = CreateDb();
        var (add, list) = CreateHandlers(db);
        await add.HandleAsync(new AddTaskInput("Task A"));
        await add.HandleAsync(new AddTaskInput("Task B"));

        var result = await list.HandleAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task HandleAsync_FilterByStatus_ReturnsMatchingTasks()
    {
        using var db = CreateDb();
        var (add, list) = CreateHandlers(db);
        var repo = new SqlTaskItemRepository(db);

        var t1 = await add.HandleAsync(new AddTaskInput("Task A"));
        t1.Status = TaskItemStatus.Done;
        await repo.UpdateAsync(t1);

        await add.HandleAsync(new AddTaskInput("Task B")); // NotStarted

        var result = await list.HandleAsync(new TaskFilter(Status: TaskItemStatus.Done));

        Assert.Single(result);
        Assert.Equal("Task A", result[0].Title);
    }

    [Fact]
    public async Task HandleAsync_FilterByPriority_ReturnsMatchingTasks()
    {
        using var db = CreateDb();
        var (add, list) = CreateHandlers(db);

        await add.HandleAsync(new AddTaskInput("Low task", Priority: TaskItemPriority.Low));
        await add.HandleAsync(new AddTaskInput("Critical task", Priority: TaskItemPriority.Critical));

        var result = await list.HandleAsync(new TaskFilter(Priority: TaskItemPriority.Critical));

        Assert.Single(result);
        Assert.Equal("Critical task", result[0].Title);
    }

    [Fact]
    public async Task HandleAsync_FilterByDeliverableTo_ReturnsMatchingTasks()
    {
        using var db = CreateDb();
        var (add, list) = CreateHandlers(db);

        await add.HandleAsync(new AddTaskInput("Task for Alice", DeliverableTo: "Alice"));
        await add.HandleAsync(new AddTaskInput("Task for Bob", DeliverableTo: "Bob"));

        var result = await list.HandleAsync(new TaskFilter(DeliverableTo: "Alice"));

        Assert.Single(result);
        Assert.Equal("Task for Alice", result[0].Title);
    }

    [Fact]
    public async Task HandleAsync_FilterByMultipleFields_AppliesAllFilters()
    {
        using var db = CreateDb();
        var (add, list) = CreateHandlers(db);
        var repo = new SqlTaskItemRepository(db);

        var t1 = await add.HandleAsync(new AddTaskInput("Match",
            Priority: TaskItemPriority.High, DeliverableTo: "Alice"));
        t1.Status = TaskItemStatus.InProgress;
        await repo.UpdateAsync(t1);

        await add.HandleAsync(new AddTaskInput("No match",
            Priority: TaskItemPriority.High, DeliverableTo: "Bob"));

        var result = await list.HandleAsync(new TaskFilter(
            Status: TaskItemStatus.InProgress,
            Priority: TaskItemPriority.High,
            DeliverableTo: "Alice"));

        Assert.Single(result);
        Assert.Equal("Match", result[0].Title);
    }

    [Fact]
    public async Task HandleAsync_NullFilter_ReturnsAll()
    {
        using var db = CreateDb();
        var (add, list) = CreateHandlers(db);
        await add.HandleAsync(new AddTaskInput("Task A"));
        await add.HandleAsync(new AddTaskInput("Task B"));

        var result = await list.HandleAsync(null);

        Assert.Equal(2, result.Count);
    }
}
