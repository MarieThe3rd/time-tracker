using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Features.Reports.DailyNote;

namespace TimeTracker.Tests.Features.Reports;

/// <summary>
/// Tests for ReportsHandler.PushWeeklySummaryAsync — uses a real temp directory for file I/O.
/// </summary>
public class ReportsHandlerWeeklyPushTests : IDisposable
{
    private readonly string _tempDir;

    public ReportsHandlerWeeklyPushTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"TimeTrackerWeeklyTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private UserSettings SettingsFor(string subfolder = "Journal\\Weekly") =>
        new() { Id = 1, VaultRootPath = _tempDir, WeeklyNotesSubfolder = subfolder };

    private static ReportsHandler CreateHandler()
    {
        var options = new DbContextOptionsBuilder<TimeTracker.Web.Data.AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new TimeTracker.Web.Data.AppDbContext(options);
        return new ReportsHandler(
            new TimeTracker.Web.Data.Repositories.Sql.SqlTimeEntryRepository(db),
            new TimeTracker.Web.Data.Repositories.Sql.SqlJournalEntryRepository(db),
            new TimeTracker.Web.Data.Repositories.Sql.SqlTaskItemRepository(db));
    }

    private static ReportRange WeekRange(DateOnly monday)
        => new(monday, monday.AddDays(6));

    [Fact]
    public async Task PushWeeklySummaryAsync_VaultPathNotConfigured_ThrowsInvalidOperationException()
    {
        var handler = CreateHandler();
        var settings = new UserSettings { Id = 1, VaultRootPath = null, WeeklyNotesSubfolder = "Journal\\Weekly" };
        var range = WeekRange(new DateOnly(2025, 5, 26));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.PushWeeklySummaryAsync(range, "# content", settings));
    }

    [Fact]
    public async Task PushWeeklySummaryAsync_EmptyVaultPath_ThrowsInvalidOperationException()
    {
        var handler = CreateHandler();
        var settings = new UserSettings { Id = 1, VaultRootPath = "   ", WeeklyNotesSubfolder = "Journal\\Weekly" };
        var range = WeekRange(new DateOnly(2025, 5, 26));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.PushWeeklySummaryAsync(range, "# content", settings));
    }

    [Fact]
    public async Task PushWeeklySummaryAsync_NewFile_WritesFullMarkdown()
    {
        var handler = CreateHandler();
        var settings = SettingsFor();
        var range = WeekRange(new DateOnly(2025, 5, 26));
        var markdown = "---\nperiod: \"May 26 – Jun 1, 2025\"\n---\n\n## 📊 Weekly Summary";

        var (filePath, overwritten) = await handler.PushWeeklySummaryAsync(range, markdown, settings);

        Assert.True(File.Exists(filePath));
        Assert.Equal(markdown, await File.ReadAllTextAsync(filePath));
        Assert.False(overwritten);
    }

    [Fact]
    public async Task PushWeeklySummaryAsync_UsesIsoWeekFilename()
    {
        var handler = CreateHandler();
        var settings = SettingsFor();
        // May 26, 2025 is ISO week 22
        var range = WeekRange(new DateOnly(2025, 5, 26));

        var (filePath, _) = await handler.PushWeeklySummaryAsync(range, "# test", settings);

        Assert.EndsWith("2025-W22.md", filePath);
    }

    [Fact]
    public async Task PushWeeklySummaryAsync_CreatesSubfolderIfMissing()
    {
        var handler = CreateHandler();
        var subfolder = Path.Combine("Journal", "Weekly");
        var settings = SettingsFor(subfolder);
        var range = WeekRange(new DateOnly(2025, 5, 26));

        await handler.PushWeeklySummaryAsync(range, "# test", settings);

        var expectedDir = Path.Combine(_tempDir, subfolder);
        Assert.True(Directory.Exists(expectedDir));
    }

    [Fact]
    public async Task PushWeeklySummaryAsync_ExistingFile_OverwritesAndReturnsOverwrittenTrue()
    {
        var handler = CreateHandler();
        var settings = SettingsFor();
        var range = WeekRange(new DateOnly(2025, 5, 26));

        // Write first time
        await handler.PushWeeklySummaryAsync(range, "# original", settings);

        // Write again
        var (filePath, overwritten) = await handler.PushWeeklySummaryAsync(range, "# updated", settings);

        Assert.True(overwritten);
        Assert.Equal("# updated", await File.ReadAllTextAsync(filePath));
    }

    [Fact]
    public async Task PushWeeklySummaryAsync_ReturnsCorrectFilePath()
    {
        var handler = CreateHandler();
        var settings = SettingsFor();
        // Jan 6, 2025 is ISO week 2
        var range = WeekRange(new DateOnly(2025, 1, 6));

        var (filePath, _) = await handler.PushWeeklySummaryAsync(range, "# test", settings);

        var expectedPath = Path.Combine(_tempDir, "Journal\\Weekly", "2025-W02.md");
        Assert.Equal(expectedPath, filePath);
    }

    [Fact]
    public async Task PushWeeklySummaryAsync_WritesToWeeklyNotesSubfolder_NotDailySubfolder()
    {
        var handler = CreateHandler();
        var settings = new UserSettings
        {
            Id = 1,
            VaultRootPath = _tempDir,
            DailyNotesSubfolder = "Journal\\Daily",
            WeeklyNotesSubfolder = "Journal\\Weekly"
        };
        var range = WeekRange(new DateOnly(2025, 5, 26));

        var (filePath, _) = await handler.PushWeeklySummaryAsync(range, "# test", settings);

        Assert.Contains("Journal\\Weekly", filePath);
        Assert.DoesNotContain("Journal\\Daily", filePath);
    }
}
