using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Reminders;

namespace TimeTracker.Tests.Features.Reminders;

public class DismissReminderHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static (AddReminderHandler add, DismissReminderHandler dismiss) CreateHandlers(AppDbContext db)
    {
        var repo = new SqlReminderRepository(db);
        return (new AddReminderHandler(repo), new DismissReminderHandler(repo));
    }

    [Fact]
    public async Task HandleAsync_SetsDismissedStatus()
    {
        using var db = CreateDb();
        var (add, dismiss) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));

        await dismiss.HandleAsync(reminder.Id);

        var saved = await db.Reminders.FindAsync(reminder.Id);
        Assert.Equal(ReminderStatus.Dismissed, saved!.Status);
    }

    [Fact]
    public async Task HandleAsync_NoRepeat_DoesNotCreateNextOccurrence()
    {
        using var db = CreateDb();
        var (add, dismiss) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput(
            "One-time", DateTime.UtcNow.AddHours(1), Repeat: ReminderRepeat.None));

        await dismiss.HandleAsync(reminder.Id);

        Assert.Equal(1, await db.Reminders.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_DailyRepeat_CreatesNextDayOccurrence()
    {
        using var db = CreateDb();
        var (add, dismiss) = CreateHandlers(db);
        var remindOn = new DateTime(2026, 3, 11, 9, 0, 0, DateTimeKind.Utc);
        var reminder = await add.HandleAsync(new AddReminderInput(
            "Daily standup", remindOn, Repeat: ReminderRepeat.Daily));

        await dismiss.HandleAsync(reminder.Id);

        var all = await db.Reminders.ToListAsync();
        Assert.Equal(2, all.Count);
        var next = all.First(r => r.Status == ReminderStatus.Active);
        Assert.Equal(remindOn.AddDays(1), next.RemindOn);
        Assert.Equal(ReminderRepeat.Daily, next.Repeat);
    }

    [Fact]
    public async Task HandleAsync_WeeklyRepeat_CreatesNextWeekOccurrence()
    {
        using var db = CreateDb();
        var (add, dismiss) = CreateHandlers(db);
        var remindOn = new DateTime(2026, 3, 11, 9, 0, 0, DateTimeKind.Utc);
        var reminder = await add.HandleAsync(new AddReminderInput(
            "Weekly review", remindOn, Repeat: ReminderRepeat.Weekly));

        await dismiss.HandleAsync(reminder.Id);

        var all = await db.Reminders.ToListAsync();
        Assert.Equal(2, all.Count);
        var next = all.First(r => r.Status == ReminderStatus.Active);
        Assert.Equal(remindOn.AddDays(7), next.RemindOn);
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb();
        var (_, dismiss) = CreateHandlers(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => dismiss.HandleAsync(9999));
    }

    [Fact]
    public async Task HandleAsync_NextOccurrence_InheritsTitle()
    {
        using var db = CreateDb();
        var (add, dismiss) = CreateHandlers(db);
        var reminder = await add.HandleAsync(new AddReminderInput(
            "Team sync", DateTime.UtcNow.AddHours(1),
            Notes: "Bring notes",
            Repeat: ReminderRepeat.Weekly));

        await dismiss.HandleAsync(reminder.Id);

        var next = await db.Reminders.FirstAsync(r => r.Status == ReminderStatus.Active);
        Assert.Equal("Team sync", next.Title);
        Assert.Equal("Bring notes", next.Notes);
    }
}
