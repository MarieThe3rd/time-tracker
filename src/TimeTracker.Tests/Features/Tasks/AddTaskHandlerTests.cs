using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Tasks;

namespace TimeTracker.Tests.Features.Tasks;

public class AddTaskHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static AddTaskHandler CreateHandler(AppDbContext db) =>
        new AddTaskHandler(new SqlTaskItemRepository(db));

    [Fact]
    public async Task HandleAsync_ValidInput_SavesTask()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new AddTaskInput("Fix the bug"));

        Assert.NotNull(result);
        Assert.Equal("Fix the bug", result.Title);
        Assert.Equal(1, await db.TaskItems.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_DefaultsToNotStarted()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new AddTaskInput("New task"));

        Assert.Equal(TaskItemStatus.NotStarted, result.Status);
    }

    [Fact]
    public async Task HandleAsync_DefaultPriorityIsMedium()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new AddTaskInput("New task"));

        Assert.Equal(TaskItemPriority.Medium, result.Priority);
    }

    [Fact]
    public async Task HandleAsync_SetsCreatedAtAndUpdatedAt()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var before = DateTime.UtcNow.AddSeconds(-1);

        var result = await handler.HandleAsync(new AddTaskInput("New task"));

        Assert.True(result.CreatedAt >= before);
        Assert.True(result.UpdatedAt >= before);
    }

    [Fact]
    public async Task HandleAsync_TrimsTitle()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new AddTaskInput("  Trimmed  "));

        Assert.Equal("Trimmed", result.Title);
    }

    [Fact]
    public async Task HandleAsync_EmptyTitle_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new AddTaskInput("")));
    }

    [Fact]
    public async Task HandleAsync_WhitespaceTitle_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new AddTaskInput("   ")));
    }

    [Fact]
    public async Task HandleAsync_WithOptionalFields_PersistedCorrectly()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var due = new DateOnly(2026, 6, 1);

        var result = await handler.HandleAsync(new AddTaskInput(
            "Deliver report",
            Description: "Q2 report",
            Priority: TaskItemPriority.High,
            DueDate: due,
            AssignedBy: "Alice",
            DeliverableTo: "Bob",
            Notes: "ASAP"));

        Assert.Equal("Q2 report", result.Description);
        Assert.Equal(TaskItemPriority.High, result.Priority);
        Assert.Equal(due, result.DueDate);
        Assert.Equal("Alice", result.AssignedBy);
        Assert.Equal("Bob", result.DeliverableTo);
        Assert.Equal("ASAP", result.Notes);
    }
}
