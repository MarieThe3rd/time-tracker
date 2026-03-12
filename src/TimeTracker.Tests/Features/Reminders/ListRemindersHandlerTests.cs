using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Reminders;
using Microsoft.EntityFrameworkCore;

namespace TimeTracker.Tests.Features.Reminders;

public class ListRemindersHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static (AddReminderHandler add, DismissReminderHandler dismiss, ListRemindersHandler list)
        CreateHandlers(AppDbContext db)
    {
        var repo = new SqlReminderRepository(db);
        return (new AddReminderHandler(repo), new DismissReminderHandler(repo), new ListRemindersHandler(repo));
    }

    [Fact]
    public async Task HandleAsync_ActiveOnly_ExcludesDismissed()
    {
        using var db = CreateDb();
        var (add, dismiss, list) = CreateHandlers(db);
        var active = await add.HandleAsync(new AddReminderInput("Active", DateTime.UtcNow.AddHours(1)));
        var toDissmiss = await add.HandleAsync(new AddReminderInput("Dismissed", DateTime.UtcNow.AddHours(2),
            Repeat: ReminderRepeat.None));
        await dismiss.HandleAsync(toDissmiss.Id);

        var result = await list.HandleAsync(activeOnly: true);

        Assert.Single(result);
        Assert.Equal("Active", result[0].Title);
    }

    [Fact]
    public async Task HandleAsync_ActiveOnly_IncludesSnoozed()
    {
        using var db = CreateDb();
        var (add, _, list) = CreateHandlers(db);
        var repo = new SqlReminderRepository(db);
        var snooze = new SnoozeReminderHandler(repo);

        var reminder = await add.HandleAsync(new AddReminderInput("Snoozed", DateTime.UtcNow.AddHours(1)));
        await snooze.HandleAsync(reminder.Id, DateTime.UtcNow.AddHours(3));

        var result = await list.HandleAsync(activeOnly: true);

        Assert.Single(result);
        Assert.Equal(ReminderStatus.Active, result[0].Status);
    }

    [Fact]
    public async Task HandleAsync_NotActiveOnly_IncludesDismissed_WhenRequested()
    {
        using var db = CreateDb();
        var (add, dismiss, list) = CreateHandlers(db);
        await add.HandleAsync(new AddReminderInput("Active", DateTime.UtcNow.AddHours(1)));
        var toDismiss = await add.HandleAsync(new AddReminderInput("Dismissed", DateTime.UtcNow.AddHours(2),
            Repeat: ReminderRepeat.None));
        await dismiss.HandleAsync(toDismiss.Id);

        var result = await list.HandleAsync(activeOnly: false, includeDismissed: true);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task HandleAsync_NotActiveOnly_ExcludesDismissed_ByDefault()
    {
        using var db = CreateDb();
        var (add, dismiss, list) = CreateHandlers(db);
        await add.HandleAsync(new AddReminderInput("Active", DateTime.UtcNow.AddHours(1)));
        var toDismiss = await add.HandleAsync(new AddReminderInput("Dismissed", DateTime.UtcNow.AddHours(2),
            Repeat: ReminderRepeat.None));
        await dismiss.HandleAsync(toDismiss.Id);

        var result = await list.HandleAsync(activeOnly: false, includeDismissed: false);

        Assert.Single(result);
        Assert.Equal("Active", result[0].Title);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyList()
    {
        using var db = CreateDb();
        var (_, _, list) = CreateHandlers(db);

        var result = await list.HandleAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_DefaultParameters_ReturnOnlyActive()
    {
        using var db = CreateDb();
        var (add, dismiss, list) = CreateHandlers(db);
        await add.HandleAsync(new AddReminderInput("Active", DateTime.UtcNow.AddHours(1)));
        var toDismiss = await add.HandleAsync(new AddReminderInput("Dismissed", DateTime.UtcNow.AddHours(2),
            Repeat: ReminderRepeat.None));
        await dismiss.HandleAsync(toDismiss.Id);

        var result = await list.HandleAsync();

        Assert.Single(result);
    }
}
