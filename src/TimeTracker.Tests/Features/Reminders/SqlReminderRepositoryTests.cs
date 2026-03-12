using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;

namespace TimeTracker.Tests.Features.Reminders;

public class SqlReminderRepositoryTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static SqlReminderRepository CreateRepo(AppDbContext db) => new(db);

    [Fact]
    public async Task GetUpcomingCountAsync_NoReminders_ReturnsZero()
    {
        using var db = CreateDb();
        var count = await CreateRepo(db).GetUpcomingCountAsync(TimeSpan.FromHours(24));

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetUpcomingCountAsync_ActiveReminderWithinWindow_Counted()
    {
        using var db = CreateDb();
        db.Reminders.Add(new Reminder
        {
            Title = "Soon",
            RemindOn = DateTime.UtcNow.AddHours(12),
            Status = ReminderStatus.Active
        });
        await db.SaveChangesAsync();

        var count = await CreateRepo(db).GetUpcomingCountAsync(TimeSpan.FromHours(24));

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GetUpcomingCountAsync_SnoozedReminderWithinWindow_Counted()
    {
        using var db = CreateDb();
        db.Reminders.Add(new Reminder
        {
            Title = "Snoozed but upcoming",
            RemindOn = DateTime.UtcNow.AddHours(6),
            Status = ReminderStatus.Snoozed
        });
        await db.SaveChangesAsync();

        var count = await CreateRepo(db).GetUpcomingCountAsync(TimeSpan.FromHours(24));

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GetUpcomingCountAsync_DismissedReminder_ExcludedFromCount()
    {
        using var db = CreateDb();
        db.Reminders.Add(new Reminder
        {
            Title = "Dismissed",
            RemindOn = DateTime.UtcNow.AddHours(1),
            Status = ReminderStatus.Dismissed
        });
        await db.SaveChangesAsync();

        var count = await CreateRepo(db).GetUpcomingCountAsync(TimeSpan.FromHours(24));

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetUpcomingCountAsync_ReminderBeyondWindow_ExcludedFromCount()
    {
        using var db = CreateDb();
        db.Reminders.Add(new Reminder
        {
            Title = "Far future",
            RemindOn = DateTime.UtcNow.AddHours(48),
            Status = ReminderStatus.Active
        });
        await db.SaveChangesAsync();

        var count = await CreateRepo(db).GetUpcomingCountAsync(TimeSpan.FromHours(24));

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetUpcomingCountAsync_PastReminder_ExcludedFromCount()
    {
        using var db = CreateDb();
        db.Reminders.Add(new Reminder
        {
            Title = "Already passed",
            RemindOn = DateTime.UtcNow.AddHours(-1),
            Status = ReminderStatus.Active
        });
        await db.SaveChangesAsync();

        var count = await CreateRepo(db).GetUpcomingCountAsync(TimeSpan.FromHours(24));

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetUpcomingCountAsync_MixedReminders_CountsOnlyActiveOrSnoozedWithinWindow()
    {
        using var db = CreateDb();
        var now = DateTime.UtcNow;
        db.Reminders.AddRange(
            new Reminder { Title = "Active soon",     RemindOn = now.AddHours(2),  Status = ReminderStatus.Active },
            new Reminder { Title = "Snoozed soon",    RemindOn = now.AddHours(10), Status = ReminderStatus.Snoozed },
            new Reminder { Title = "Dismissed soon",  RemindOn = now.AddHours(3),  Status = ReminderStatus.Dismissed },
            new Reminder { Title = "Active too far",  RemindOn = now.AddHours(30), Status = ReminderStatus.Active },
            new Reminder { Title = "Active in past",  RemindOn = now.AddHours(-2), Status = ReminderStatus.Active }
        );
        await db.SaveChangesAsync();

        var count = await CreateRepo(db).GetUpcomingCountAsync(TimeSpan.FromHours(24));

        Assert.Equal(2, count);
    }
}
