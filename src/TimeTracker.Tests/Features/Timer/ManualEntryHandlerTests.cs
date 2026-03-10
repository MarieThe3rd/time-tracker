using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Features.Timer.ManualEntry;

namespace TimeTracker.Tests.Features.Timer;

public class ManualEntryHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_ValidInput_SavesAndReturnsEntry()
    {
        using var db = CreateDb();
        var handler = new ManualEntryHandler(db);
        var start = DateTime.UtcNow.AddHours(-2);
        var end = DateTime.UtcNow.AddHours(-1);
        var input = new ManualEntryInput(start, end, null, "Planning session", 4, "Planned sprint tasks", false, false, null, null);

        var entry = await handler.HandleAsync(input);

        Assert.NotNull(entry);
        Assert.Equal(start, entry.StartTime);
        Assert.Equal(end, entry.EndTime);
        Assert.Equal("Planning session", entry.Description);
        Assert.Equal(4, entry.ProductivityRating);
        Assert.Equal("Planned sprint tasks", entry.ValueAdded);
        Assert.Equal(1, await db.TimeEntries.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_EndBeforeStart_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = new ManualEntryHandler(db);
        var input = new ManualEntryInput(
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(-1), // end before start
            null, null, null, null, false, false, null, null);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(input));
    }

    [Fact]
    public async Task HandleAsync_WithProductivityRating_PersistsRating()
    {
        using var db = CreateDb();
        var handler = new ManualEntryHandler(db);
        var input = new ManualEntryInput(
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow,
            null, "Deep work", 5, null, false, false, null, null);

        var entry = await handler.HandleAsync(input);

        Assert.Equal(5, entry.ProductivityRating);
    }

    [Fact]
    public async Task HandleAsync_EndTimeEqualsStartTime_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = new ManualEntryHandler(db);
        var time = DateTime.UtcNow;
        var input = new ManualEntryInput(time, time, null, null, null, null, false, false, null, null);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(input));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task HandleAsync_ProductivityRatingOutOfRange_ThrowsArgumentOutOfRangeException(int rating)
    {
        using var db = CreateDb();
        var handler = new ManualEntryHandler(db);
        var input = new ManualEntryInput(
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow,
            null, null, rating, null, false, false, null, null);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => handler.HandleAsync(input));
    }

    [Fact]
    public async Task HandleAsync_NullProductivityRating_SavesSuccessfully()
    {
        using var db = CreateDb();
        var handler = new ManualEntryHandler(db);
        var input = new ManualEntryInput(
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow,
            null, "No rating", null, null, false, false, null, null);

        var entry = await handler.HandleAsync(input);

        Assert.Null(entry.ProductivityRating);
    }

    [Fact]
    public async Task HandleAsync_AllNullOptionalFields_SavesSuccessfully()
    {
        using var db = CreateDb();
        var handler = new ManualEntryHandler(db);
        var input = new ManualEntryInput(
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow,
            null, null, null, null, false, false, null, null);

        var entry = await handler.HandleAsync(input);

        Assert.NotNull(entry);
        Assert.Null(entry.WorkCategoryId);
        Assert.Null(entry.Description);
        Assert.Null(entry.ProductivityRating);
    }

    [Fact]
    public async Task HandleAsync_BreakEntryWithProductivity_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = new ManualEntryHandler(db);
        var input = new ManualEntryInput(
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow,
            null, "Break", 3, null, true, false, null, null);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(input));
    }

    [Fact]
    public async Task HandleAsync_AiUsedWithoutDetails_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = new ManualEntryHandler(db);
        var input = new ManualEntryInput(
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow,
            null, "AI task", null, "Automated tests", false, true, null, null);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(input));
    }

    [Fact]
    public async Task HandleAsync_AiUsedWithDetails_PersistsAiFields()
    {
        using var db = CreateDb();
        var handler = new ManualEntryHandler(db);
        var input = new ManualEntryInput(
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow,
            null, "Refactor", 4, "Reduced boilerplate", false, true, 25, "Used Copilot to scaffold repetitive mapping code.");

        var entry = await handler.HandleAsync(input);

        Assert.True(entry.AiUsed);
        Assert.Equal(25, entry.AiTimeSavedMinutes);
        Assert.Equal("Used Copilot to scaffold repetitive mapping code.", entry.AiNotes);
    }
}
