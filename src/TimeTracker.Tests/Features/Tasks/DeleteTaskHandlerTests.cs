using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Tasks;

namespace TimeTracker.Tests.Features.Tasks;

public class DeleteTaskHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static (AddTaskHandler add, DeleteTaskHandler delete) CreateHandlers(AppDbContext db)
    {
        var repo = new SqlTaskItemRepository(db);
        return (new AddTaskHandler(repo), new DeleteTaskHandler(repo));
    }

    [Fact]
    public async Task HandleAsync_ExistingTask_RemovesFromDatabase()
    {
        using var db = CreateDb();
        var (add, delete) = CreateHandlers(db);
        var task = await add.HandleAsync(new AddTaskInput("To be deleted"));

        await delete.HandleAsync(task.Id);

        Assert.Equal(0, await db.TaskItems.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_OnlyDeletesTargetTask()
    {
        using var db = CreateDb();
        var (add, delete) = CreateHandlers(db);
        var taskA = await add.HandleAsync(new AddTaskInput("Keep me"));
        var taskB = await add.HandleAsync(new AddTaskInput("Delete me"));

        await delete.HandleAsync(taskB.Id);

        Assert.Equal(1, await db.TaskItems.CountAsync());
        var remaining = await db.TaskItems.FirstAsync();
        Assert.Equal("Keep me", remaining.Title);
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_DoesNotThrow()
    {
        using var db = CreateDb();
        var (_, delete) = CreateHandlers(db);

        var exception = await Record.ExceptionAsync(() => delete.HandleAsync(9999));

        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_AfterDelete_TaskNoLongerRetrievable()
    {
        using var db = CreateDb();
        var (add, delete) = CreateHandlers(db);
        var task = await add.HandleAsync(new AddTaskInput("Goodbye"));

        await delete.HandleAsync(task.Id);

        var found = await db.TaskItems.FindAsync(task.Id);
        Assert.Null(found);
    }
}
