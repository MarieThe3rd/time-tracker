using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Features.Reports.DailyNote;

namespace TimeTracker.Tests.Features.Reports;

/// <summary>
/// Tests for ReportsHandler.PushAiUsageReportAsync — uses a real temp directory for file I/O.
/// </summary>
public class ReportsHandlerAiUsagePushTests : IDisposable
{
    private readonly string _tempDir;

    public ReportsHandlerAiUsagePushTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"TimeTrackerAiTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private UserSettings SettingsFor(string? vaultRootPath = null) =>
        new() { Id = 1, VaultRootPath = vaultRootPath ?? _tempDir };

    private static ReportsHandler CreateHandler()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        return new ReportsHandler(
            new TimeTracker.Web.Data.Repositories.Sql.SqlTimeEntryRepository(db),
            new TimeTracker.Web.Data.Repositories.Sql.SqlJournalEntryRepository(db),
            new TimeTracker.Web.Data.Repositories.Sql.SqlTaskItemRepository(db));
    }

    // -------------------------------------------------------------------------
    // 1. Creates file when it does not exist
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PushAiUsageReportAsync_NewFile_CreatesFileWithCorrectContent()
    {
        var handler = CreateHandler();
        var settings = SettingsFor();
        var from = new DateOnly(2026, 1, 5);
        var to = new DateOnly(2026, 1, 31);
        var markdown = "# AI Usage Report\n\nSome content here.";

        var (filePath, overwritten) = await handler.PushAiUsageReportAsync(from, to, markdown, settings);

        Assert.True(File.Exists(filePath));
        Assert.Equal(markdown, await File.ReadAllTextAsync(filePath));
        Assert.False(overwritten);
    }

    // -------------------------------------------------------------------------
    // 2. Overwrites file and returns Overwritten = true
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PushAiUsageReportAsync_ExistingFile_OverwritesContentAndReturnsTrue()
    {
        var handler = CreateHandler();
        var settings = SettingsFor();
        var from = new DateOnly(2026, 1, 5);
        var to = new DateOnly(2026, 1, 31);

        // Pre-create the file with old content
        var expectedFileName = $"AI-Usage-Report-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}.md";
        var existingFilePath = Path.Combine(_tempDir, expectedFileName);
        await File.WriteAllTextAsync(existingFilePath, "OLD CONTENT");

        var newMarkdown = "# AI Usage Report\n\nNEW CONTENT";
        var (filePath, overwritten) = await handler.PushAiUsageReportAsync(from, to, newMarkdown, settings);

        Assert.True(overwritten);
        Assert.Equal(newMarkdown, await File.ReadAllTextAsync(filePath));
        Assert.DoesNotContain("OLD CONTENT", await File.ReadAllTextAsync(filePath));
    }

    // -------------------------------------------------------------------------
    // 3a. Throws InvalidOperationException when VaultRootPath is null
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PushAiUsageReportAsync_NullVaultPath_ThrowsInvalidOperationException()
    {
        var handler = CreateHandler();
        var settings = new UserSettings { Id = 1, VaultRootPath = null };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.PushAiUsageReportAsync(
                new DateOnly(2026, 1, 5),
                new DateOnly(2026, 1, 31),
                "# content",
                settings));

        Assert.Contains("Vault root path is not configured", ex.Message);
    }

    // -------------------------------------------------------------------------
    // 3b. Throws InvalidOperationException when VaultRootPath is whitespace
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PushAiUsageReportAsync_WhitespaceVaultPath_ThrowsInvalidOperationException()
    {
        var handler = CreateHandler();
        var settings = new UserSettings { Id = 1, VaultRootPath = "   " };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.PushAiUsageReportAsync(
                new DateOnly(2026, 1, 5),
                new DateOnly(2026, 1, 31),
                "# content",
                settings));

        Assert.Contains("Vault root path is not configured", ex.Message);
    }

    // -------------------------------------------------------------------------
    // 4. Creates directory if it does not exist
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PushAiUsageReportAsync_VaultDirectoryMissing_CreatesDirectory()
    {
        var handler = CreateHandler();
        // Use a subdirectory that does not yet exist
        var nonExistentDir = Path.Combine(_tempDir, "new-vault-dir");
        var settings = SettingsFor(nonExistentDir);

        Assert.False(Directory.Exists(nonExistentDir));

        await handler.PushAiUsageReportAsync(
            new DateOnly(2026, 1, 5),
            new DateOnly(2026, 1, 31),
            "# content",
            settings);

        Assert.True(Directory.Exists(nonExistentDir));
    }

    // -------------------------------------------------------------------------
    // 5. File path contains correct date range in the filename
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PushAiUsageReportAsync_ReturnsPathWithCorrectDateRangeInFilename()
    {
        var handler = CreateHandler();
        var settings = SettingsFor();
        var from = new DateOnly(2026, 1, 5);
        var to = new DateOnly(2026, 1, 31);

        var (filePath, _) = await handler.PushAiUsageReportAsync(from, to, "# content", settings);

        Assert.EndsWith("AI-Usage-Report-2026-01-05-to-2026-01-31.md", filePath);
    }
}
