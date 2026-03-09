using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Features.Timer;

namespace TimeTracker.Tests.Features.Timer;

public class DeleteTimeEntryHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_ExistingEntry_DeletesIt()
    {
        using var db = CreateDb();
        db.TimeEntries.Add(new TimeEntry
        {
            Id = 1,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow.AddHours(-1)
        });
        await db.SaveChangesAsync();

        var handler = new DeleteTimeEntryHandler(db);
        await handler.HandleAsync(1);

        Assert.Equal(0, await db.TimeEntries.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_EntryIsActuallyRemoved_FromDatabase()
    {
        using var db = CreateDb();
        db.TimeEntries.Add(new TimeEntry
        {
            Id = 10,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new DeleteTimeEntryHandler(db);
        await handler.HandleAsync(10);

        var found = await db.TimeEntries.FindAsync(10);
        Assert.Null(found);
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_DoesNotThrow()
    {
        using var db = CreateDb();
        var handler = new DeleteTimeEntryHandler(db);

        var ex = await Record.ExceptionAsync(() => handler.HandleAsync(999));

        Assert.Null(ex);
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_LeavesOtherEntriesUntouched()
    {
        using var db = CreateDb();
        db.TimeEntries.Add(new TimeEntry
        {
            Id = 5,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new DeleteTimeEntryHandler(db);
        await handler.HandleAsync(999);

        Assert.Equal(1, await db.TimeEntries.CountAsync());
    }
}
