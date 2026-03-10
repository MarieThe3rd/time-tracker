using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Features.Reports.AiUsage;
using Xunit;

namespace TimeTracker.Tests.Features.Reports;

public class AiUsageReportHandlerTests
{
  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new AppDbContext(options);
  }

  [Fact]
  public async Task GetAiUsageAsync_Returns_Only_AiUsed_Entries_In_Range()
  {
    using var db = CreateDbContext();
    db.TimeEntries.AddRange(new[]
    {
            new TimeEntry { Id = 1, StartTime = DateTime.UtcNow.AddDays(-2), EndTime = DateTime.UtcNow.AddDays(-2).AddHours(1), AiUsed = true, AiTimeSavedMinutes = 10, AiNotes = "Test1" },
            new TimeEntry { Id = 2, StartTime = DateTime.UtcNow.AddDays(-1), EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1), AiUsed = false },
            new TimeEntry { Id = 3, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), AiUsed = true, AiTimeSavedMinutes = 20, AiNotes = "Test2" }
        });
    await db.SaveChangesAsync();
    var handler = new AiUsageReportHandler(db);
    var from = DateTime.UtcNow.AddDays(-3);
    var to = DateTime.UtcNow.AddDays(1);
    var result = await handler.GetAiUsageAsync(from, to);
    Assert.Equal(2, result.Count);
    Assert.All(result, x => Assert.True(x.AiTimeSavedMinutes == 10 || x.AiTimeSavedMinutes == 20));
  }
}
