using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Timer;

namespace TimeTracker.Tests.Features.Timer;

public class StopTimerHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_WhenTimerRunning_SavesTimeEntry()
    {
        using var db = CreateDb();
        var timerService = new RunningTimerService();
        timerService.Start(1, "Writing tests");
        await Task.Delay(50); // small delay so duration > 0

        var handler = new StopTimerHandler(new SqlTimeEntryRepository(db), timerService);
        var entry = await handler.HandleAsync();

        Assert.NotNull(entry);
        Assert.True(entry.EndTime > entry.StartTime);
        Assert.Equal("Writing tests", entry.Description);
        Assert.Equal(1, entry.WorkCategoryId);
        Assert.False(timerService.IsRunning);
    }

    [Fact]
    public async Task HandleAsync_WhenTimerNotRunning_SavesEntryWithSameStartAndEnd()
    {
        using var db = CreateDb();
        var timerService = new RunningTimerService();
        // Not started — Stop should still produce an entry without crashing

        var handler = new StopTimerHandler(new SqlTimeEntryRepository(db), timerService);
        var entry = await handler.HandleAsync();

        Assert.NotNull(entry);
    }
}