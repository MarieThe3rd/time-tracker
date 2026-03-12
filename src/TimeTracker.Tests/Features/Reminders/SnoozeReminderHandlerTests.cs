using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Reminders;

namespace TimeTracker.Tests.Features.Reminders;

public class SnoozeReminderHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static (AddReminderHandler add, SnoozeReminderHandler snooze) CreateHandlers(AppDbContext db)
    {
        var repo = new SqlReminderRepository(db);
        return (new AddReminderHandler(repo), new SnoozeReminderHandler(repo));
    }

    [Fact]
    public async Task HandleAsync_UpdatesRemindOn()
    {
        using var db = CreateDb();
        var (add, snooze) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));
        var newTime = DateTime.UtcNow.AddHours(4);

        await snooze.HandleAsync(reminder.Id, newTime);

        var saved = await db.Reminders.FindAsync(reminder.Id);
        Assert.Equal(newTime, saved!.RemindOn);
    }

    [Fact]
    public async Task HandleAsync_SetsStatusToActive()
    {
        using var db = CreateDb();
        var (add, snooze) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));

        await snooze.HandleAsync(reminder.Id, DateTime.UtcNow.AddHours(4));

        var saved = await db.Reminders.FindAsync(reminder.Id);
        Assert.Equal(ReminderStatus.Active, saved!.Status);
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb();
        var (_, snooze) = CreateHandlers(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            snooze.HandleAsync(9999, DateTime.UtcNow.AddHours(1)));
    }

    [Fact]
    public async Task HandleAsync_DefaultNewRemindOn_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var (add, snooze) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            snooze.HandleAsync(reminder.Id, default));
    }

    [Fact]
    public async Task HandleAsync_DoesNotCreateNewRecord()
    {
        using var db = CreateDb();
        var (add, snooze) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));

        await snooze.HandleAsync(reminder.Id, DateTime.UtcNow.AddHours(4));

        Assert.Equal(1, await db.Reminders.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_PreservesOtherFields()
    {
        using var db = CreateDb();
        var (add, snooze) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput(
            "Weekly review", DateTime.UtcNow.AddHours(1),
            Notes: "Check backlog",
            Repeat: ReminderRepeat.Weekly));

        await snooze.HandleAsync(reminder.Id, DateTime.UtcNow.AddHours(8));

        var saved = await db.Reminders.FindAsync(reminder.Id);
        Assert.Equal("Weekly review", saved!.Title);
        Assert.Equal("Check backlog", saved.Notes);
        Assert.Equal(ReminderRepeat.Weekly, saved.Repeat);
    }
}
