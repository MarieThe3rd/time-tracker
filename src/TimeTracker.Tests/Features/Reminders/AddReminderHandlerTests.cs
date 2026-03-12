using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Reminders;

namespace TimeTracker.Tests.Features.Reminders;

public class AddReminderHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static AddReminderHandler CreateHandler(AppDbContext db) =>
        new AddReminderHandler(new SqlReminderRepository(db));

    [Fact]
    public async Task HandleAsync_ValidInput_SavesReminder()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var remindOn = DateTime.UtcNow.AddHours(1);

        var result = await handler.HandleAsync(new AddReminderInput("Stand-up", remindOn));

        Assert.NotNull(result);
        Assert.Equal("Stand-up", result.Title);
        Assert.Equal(1, await db.Reminders.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_DefaultsToActiveStatus()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));

        Assert.Equal(ReminderStatus.Active, result.Status);
    }

    [Fact]
    public async Task HandleAsync_DefaultsToNoRepeat()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));

        Assert.Equal(ReminderRepeat.None, result.Repeat);
    }

    [Fact]
    public async Task HandleAsync_EmptyTitle_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new AddReminderInput("", DateTime.UtcNow.AddHours(1))));
    }

    [Fact]
    public async Task HandleAsync_WhitespaceTitle_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new AddReminderInput("   ", DateTime.UtcNow.AddHours(1))));
    }

    [Fact]
    public async Task HandleAsync_DefaultRemindOn_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new AddReminderInput("Test", default)));
    }

    [Fact]
    public async Task HandleAsync_WithRepeat_PersistedCorrectly()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new AddReminderInput(
            "Daily standup", DateTime.UtcNow.AddHours(1), Repeat: ReminderRepeat.Daily));

        Assert.Equal(ReminderRepeat.Daily, result.Repeat);
    }

    [Fact]
    public async Task HandleAsync_SetsCreatedAt()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var before = DateTime.UtcNow.AddSeconds(-1);

        var result = await handler.HandleAsync(new AddReminderInput("Test", DateTime.UtcNow.AddHours(1)));

        Assert.True(result.CreatedAt >= before);
    }
}
