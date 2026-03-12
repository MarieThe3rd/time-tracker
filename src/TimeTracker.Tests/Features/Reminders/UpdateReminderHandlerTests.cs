using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Reminders;

namespace TimeTracker.Tests.Features.Reminders;

public class UpdateReminderHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static (AddReminderHandler add, UpdateReminderHandler update) CreateHandlers(AppDbContext db)
    {
        var repo = new SqlReminderRepository(db);
        return (new AddReminderHandler(repo), new UpdateReminderHandler(repo));
    }

    [Fact]
    public async Task HandleAsync_UpdatesTitle()
    {
        using var db = CreateDb();
        var (add, update) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Original", DateTime.UtcNow.AddHours(1)));

        await update.HandleAsync(new UpdateReminderInput(
            reminder.Id, "Updated", reminder.RemindOn, ReminderRepeat.None, ReminderStatus.Active));

        var saved = await db.Reminders.FindAsync(reminder.Id);
        Assert.Equal("Updated", saved!.Title);
    }

    [Fact]
    public async Task HandleAsync_TrimsTitle()
    {
        using var db = CreateDb();
        var (add, update) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Original", DateTime.UtcNow.AddHours(1)));

        await update.HandleAsync(new UpdateReminderInput(
            reminder.Id, "  Trimmed  ", reminder.RemindOn, ReminderRepeat.None, ReminderStatus.Active));

        var saved = await db.Reminders.FindAsync(reminder.Id);
        Assert.Equal("Trimmed", saved!.Title);
    }

    [Fact]
    public async Task HandleAsync_UpdatesRemindOn()
    {
        using var db = CreateDb();
        var (add, update) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));
        var newTime = new DateTime(2027, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        await update.HandleAsync(new UpdateReminderInput(
            reminder.Id, reminder.Title, newTime, ReminderRepeat.None, ReminderStatus.Active));

        var saved = await db.Reminders.FindAsync(reminder.Id);
        Assert.Equal(newTime, saved!.RemindOn);
    }

    [Fact]
    public async Task HandleAsync_UpdatesRepeat()
    {
        using var db = CreateDb();
        var (add, update) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));

        await update.HandleAsync(new UpdateReminderInput(
            reminder.Id, reminder.Title, reminder.RemindOn, ReminderRepeat.Weekly, ReminderStatus.Active));

        var saved = await db.Reminders.FindAsync(reminder.Id);
        Assert.Equal(ReminderRepeat.Weekly, saved!.Repeat);
    }

    [Fact]
    public async Task HandleAsync_UpdatesStatus()
    {
        using var db = CreateDb();
        var (add, update) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));

        await update.HandleAsync(new UpdateReminderInput(
            reminder.Id, reminder.Title, reminder.RemindOn, ReminderRepeat.None, ReminderStatus.Snoozed));

        var saved = await db.Reminders.FindAsync(reminder.Id);
        Assert.Equal(ReminderStatus.Snoozed, saved!.Status);
    }

    [Fact]
    public async Task HandleAsync_UpdatesNotes()
    {
        using var db = CreateDb();
        var (add, update) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));

        await update.HandleAsync(new UpdateReminderInput(
            reminder.Id, reminder.Title, reminder.RemindOn, ReminderRepeat.None, ReminderStatus.Active,
            Notes: "New notes"));

        var saved = await db.Reminders.FindAsync(reminder.Id);
        Assert.Equal("New notes", saved!.Notes);
    }

    [Fact]
    public async Task HandleAsync_EmptyTitle_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var (add, update) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            update.HandleAsync(new UpdateReminderInput(
                reminder.Id, "", reminder.RemindOn, ReminderRepeat.None, ReminderStatus.Active)));
    }

    [Fact]
    public async Task HandleAsync_WhitespaceTitle_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var (add, update) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            update.HandleAsync(new UpdateReminderInput(
                reminder.Id, "   ", reminder.RemindOn, ReminderRepeat.None, ReminderStatus.Active)));
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb();
        var (_, update) = CreateHandlers(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            update.HandleAsync(new UpdateReminderInput(
                9999, "Title", DateTime.UtcNow.AddHours(1), ReminderRepeat.None, ReminderStatus.Active)));
    }
}
