using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;

namespace TimeTracker.Tests.Features.Tasks;

public class SqlTaskItemRepositoryTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static SqlTaskItemRepository CreateRepo(AppDbContext db) => new(db);

    private static DateOnly Yesterday => DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
    private static DateOnly Tomorrow => DateOnly.FromDateTime(DateTime.Today).AddDays(1);

    [Fact]
    public async Task GetOverdueCountAsync_NoTasks_ReturnsZero()
    {
        using var db = CreateDb();
        var count = await CreateRepo(db).GetOverdueCountAsync();

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetOverdueCountAsync_TasksWithPastDueDate_ReturnsCorrectCount()
    {
        using var db = CreateDb();
        db.TaskItems.AddRange(
            new TaskItem { Title = "A", DueDate = Yesterday, Status = TaskItemStatus.NotStarted },
            new TaskItem { Title = "B", DueDate = Yesterday, Status = TaskItemStatus.InProgress },
            new TaskItem { Title = "C", DueDate = Yesterday, Status = TaskItemStatus.Blocked }
        );
        await db.SaveChangesAsync();

        var count = await CreateRepo(db).GetOverdueCountAsync();

        Assert.Equal(3, count);
    }

    [Fact]
    public async Task GetOverdueCountAsync_DoneTaskWithPastDueDate_ExcludedFromCount()
    {
        using var db = CreateDb();
        db.TaskItems.AddRange(
            new TaskItem { Title = "Done overdue", DueDate = Yesterday, Status = TaskItemStatus.Done },
            new TaskItem { Title = "Still overdue", DueDate = Yesterday, Status = TaskItemStatus.InProgress }
        );
        await db.SaveChangesAsync();

        var count = await CreateRepo(db).GetOverdueCountAsync();

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GetOverdueCountAsync_TaskWithNoDueDate_ExcludedFromCount()
    {
        using var db = CreateDb();
        db.TaskItems.Add(new TaskItem { Title = "No due date", DueDate = null, Status = TaskItemStatus.NotStarted });
        await db.SaveChangesAsync();

        var count = await CreateRepo(db).GetOverdueCountAsync();

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetOverdueCountAsync_TaskDueTodayOrInFuture_NotCountedAsOverdue()
    {
        using var db = CreateDb();
        db.TaskItems.AddRange(
            new TaskItem { Title = "Due today", DueDate = Today, Status = TaskItemStatus.NotStarted },
            new TaskItem { Title = "Due tomorrow", DueDate = Tomorrow, Status = TaskItemStatus.NotStarted }
        );
        await db.SaveChangesAsync();

        var count = await CreateRepo(db).GetOverdueCountAsync();

        Assert.Equal(0, count);
    }
}
