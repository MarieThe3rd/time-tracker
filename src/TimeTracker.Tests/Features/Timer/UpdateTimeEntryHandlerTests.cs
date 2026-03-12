using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Timer;

namespace TimeTracker.Tests.Features.Timer;

public class UpdateTimeEntryHandlerTests
{
  private static AppDbContext CreateDb()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new AppDbContext(options);
  }

  [Fact]
  public async Task HandleAsync_ExistingEntry_UpdatesEditableFields()
  {
    using var db = CreateDb();
    db.TimeEntries.Add(new TimeEntry
    {
      Id = 1,
      StartTime = new DateTime(2026, 3, 9, 13, 0, 0, DateTimeKind.Utc),
      EndTime = new DateTime(2026, 3, 9, 14, 0, 0, DateTimeKind.Utc),
      WorkCategoryId = 1,
      Description = "Before",
      ProductivityRating = 2
    });
    await db.SaveChangesAsync();

    var handler = new UpdateTimeEntryHandler(new SqlTimeEntryRepository(db));
    var updated = await handler.HandleAsync(new UpdateTimeEntryInput(
        1,
        new DateTime(2026, 3, 9, 15, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 3, 9, 16, 0, 0, DateTimeKind.Utc),
        2,
        "After",
        4,
        "Shipped improvement",
        false,
        true,
        30,
        "Prompted for edge-case generation."));

    Assert.NotNull(updated);
    Assert.Equal(2, updated!.WorkCategoryId);
    Assert.Equal("After", updated.Description);
    Assert.Equal(4, updated.ProductivityRating);
    Assert.Equal("Shipped improvement", updated.ValueAdded);
    Assert.True(updated.AiUsed);
    Assert.Equal(30, updated.AiTimeSavedMinutes);
  }

  [Fact]
  public async Task HandleAsync_NonExistentEntry_ReturnsNull()
  {
    using var db = CreateDb();
    var handler = new UpdateTimeEntryHandler(new SqlTimeEntryRepository(db));

    var result = await handler.HandleAsync(new UpdateTimeEntryInput(
        999,
        DateTime.UtcNow.AddHours(-1),
        DateTime.UtcNow,
        null,
        null,
        null,
        null,
        false,
        false,
        null,
        null));

    Assert.Null(result);
  }

  [Fact]
  public async Task HandleAsync_BreakEntry_ClearsProductivity()
  {
    using var db = CreateDb();
    db.TimeEntries.Add(new TimeEntry
    {
      Id = 2,
      StartTime = DateTime.UtcNow.AddHours(-2),
      EndTime = DateTime.UtcNow.AddHours(-1),
      ProductivityRating = 5
    });
    await db.SaveChangesAsync();

    var handler = new UpdateTimeEntryHandler(new SqlTimeEntryRepository(db));
    var result = await handler.HandleAsync(new UpdateTimeEntryInput(
        2,
        DateTime.UtcNow.AddHours(-2),
        DateTime.UtcNow.AddHours(-1),
        null,
        "Break",
        null,
        null,
        true,
        false,
        null,
        null));

    Assert.NotNull(result);
    Assert.True(result!.IsBreak);
    Assert.Null(result.ProductivityRating);
  }

  [Fact]
  public async Task HandleAsync_AiUsedMissingNotes_ThrowsArgumentException()
  {
    using var db = CreateDb();
    db.TimeEntries.Add(new TimeEntry
    {
      Id = 3,
      StartTime = DateTime.UtcNow.AddHours(-2),
      EndTime = DateTime.UtcNow.AddHours(-1)
    });
    await db.SaveChangesAsync();

    var handler = new UpdateTimeEntryHandler(new SqlTimeEntryRepository(db));
    await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(new UpdateTimeEntryInput(
        3,
        DateTime.UtcNow.AddHours(-2),
        DateTime.UtcNow.AddHours(-1),
        null,
        "Task",
        3,
        null,
        false,
        true,
        10,
        null)));
  }
}