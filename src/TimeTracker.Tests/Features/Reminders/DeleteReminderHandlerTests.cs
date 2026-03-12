using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Reminders;

namespace TimeTracker.Tests.Features.Reminders;

public class DeleteReminderHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static (AddReminderHandler add, DeleteReminderHandler delete) CreateHandlers(AppDbContext db)
    {
        var repo = new SqlReminderRepository(db);
        return (new AddReminderHandler(repo), new DeleteReminderHandler(repo));
    }

    [Fact]
    public async Task HandleAsync_ExistingReminder_RemovesFromDatabase()
    {
        using var db = CreateDb();
        var (add, delete) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("To be deleted", DateTime.UtcNow.AddHours(1)));

        await delete.HandleAsync(reminder.Id);

        Assert.Equal(0, await db.Reminders.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_OnlyDeletesTargetReminder()
    {
        using var db = CreateDb();
        var (add, delete) = CreateHandlers(db);
        var reminderA = await add.HandleAsync(new AddReminderInput("Keep me", DateTime.UtcNow.AddHours(1)));
        var reminderB = await add.HandleAsync(new AddReminderInput("Delete me", DateTime.UtcNow.AddHours(2)));

        await delete.HandleAsync(reminderB.Id);

        Assert.Equal(1, await db.Reminders.CountAsync());
        var remaining = await db.Reminders.FirstAsync();
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
    public async Task HandleAsync_AfterDelete_ReminderNoLongerRetrievable()
    {
        using var db = CreateDb();
        var (add, delete) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Goodbye", DateTime.UtcNow.AddHours(1)));

        await delete.HandleAsync(reminder.Id);

        var found = await db.Reminders.FindAsync(reminder.Id);
        Assert.Null(found);
    }
}
