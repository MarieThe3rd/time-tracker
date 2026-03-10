using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Features.Timer;

namespace TimeTracker.Tests.Features.Timer;

public class UpdateProductivityHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task HandleAsync_ValidRating_PersistsRating(int rating)
    {
        using var db = CreateDb();
        db.TimeEntries.Add(new TimeEntry
        {
            Id = 1,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new UpdateProductivityHandler(db);
        await handler.HandleAsync(1, rating);

        var entry = await db.TimeEntries.FindAsync(1);
        Assert.Equal(rating, entry!.ProductivityRating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(100)]
    public async Task HandleAsync_OutOfBoundsRating_ThrowsArgumentOutOfRangeException(int rating)
    {
        using var db = CreateDb();
        db.TimeEntries.Add(new TimeEntry
        {
            Id = 1,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new UpdateProductivityHandler(db);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => handler.HandleAsync(1, rating));
    }

    [Fact]
    public async Task HandleAsync_NonExistentEntry_DoesNotThrow()
    {
        using var db = CreateDb();
        var handler = new UpdateProductivityHandler(db);

        var ex = await Record.ExceptionAsync(() => handler.HandleAsync(999, 3));

        Assert.Null(ex);
    }

    [Fact]
    public async Task HandleAsync_ValidRating_SavesChangesToDatabase()
    {
        using var db = CreateDb();
        db.TimeEntries.Add(new TimeEntry
        {
            Id = 2,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow.AddHours(-1)
        });
        await db.SaveChangesAsync();

        var handler = new UpdateProductivityHandler(db);
        await handler.HandleAsync(2, 4);

        // Reload from DB to verify persistence
        db.ChangeTracker.Clear();
        var reloaded = await db.TimeEntries.FindAsync(2);
        Assert.Equal(4, reloaded!.ProductivityRating);
    }

    [Fact]
    public async Task HandleAsync_UpdatesExistingRating_WithNewValue()
    {
        using var db = CreateDb();
        db.TimeEntries.Add(new TimeEntry
        {
            Id = 3,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow,
            ProductivityRating = 2
        });
        await db.SaveChangesAsync();

        var handler = new UpdateProductivityHandler(db);
        await handler.HandleAsync(3, 5);

        var entry = await db.TimeEntries.FindAsync(3);
        Assert.Equal(5, entry!.ProductivityRating);
    }

    [Fact]
    public async Task HandleAsync_BreakEntry_DoesNotSetProductivity()
    {
        using var db = CreateDb();
        db.TimeEntries.Add(new TimeEntry
        {
            Id = 4,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow,
            IsBreak = true
        });
        await db.SaveChangesAsync();

        var handler = new UpdateProductivityHandler(db);
        await handler.HandleAsync(4, 5);

        var entry = await db.TimeEntries.FindAsync(4);
        Assert.Null(entry!.ProductivityRating);
    }
}
