using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Tasks;

namespace TimeTracker.Tests.Features.Tasks;

public class GetTaskHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static (AddTaskHandler add, GetTaskHandler get) CreateHandlers(AppDbContext db)
    {
        var repo = new SqlTaskItemRepository(db);
        return (new AddTaskHandler(repo), new GetTaskHandler(repo));
    }

    [Fact]
    public async Task HandleAsync_ExistingId_ReturnsTask()
    {
        using var db = CreateDb();
        var (add, get) = CreateHandlers(db);
        var created = await add.HandleAsync(new AddTaskInput("Find me"));

        var result = await get.HandleAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal("Find me", result.Title);
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_ReturnsNull()
    {
        using var db = CreateDb();
        var (_, get) = CreateHandlers(db);

        var result = await get.HandleAsync(9999);

        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_ReturnsCorrectTask_WhenMultipleExist()
    {
        using var db = CreateDb();
        var (add, get) = CreateHandlers(db);
        await add.HandleAsync(new AddTaskInput("Task A"));
        var taskB = await add.HandleAsync(new AddTaskInput("Task B"));
        await add.HandleAsync(new AddTaskInput("Task C"));

        var result = await get.HandleAsync(taskB.Id);

        Assert.NotNull(result);
        Assert.Equal("Task B", result.Title);
    }

    [Fact]
    public async Task HandleAsync_ReturnsTaskWithAllFields()
    {
        using var db = CreateDb();
        var (add, get) = CreateHandlers(db);
        var due = new DateOnly(2026, 12, 31);
        var created = await add.HandleAsync(new AddTaskInput(
            "Full task",
            Description: "Details here",
            Priority: TaskItemPriority.High,
            DueDate: due,
            AssignedBy: "Alice",
            DeliverableTo: "Bob",
            Notes: "Important"));

        var result = await get.HandleAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal("Full task", result.Title);
        Assert.Equal("Details here", result.Description);
        Assert.Equal(TaskItemPriority.High, result.Priority);
        Assert.Equal(due, result.DueDate);
        Assert.Equal("Alice", result.AssignedBy);
        Assert.Equal("Bob", result.DeliverableTo);
        Assert.Equal("Important", result.Notes);
    }
}
