using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Features.Reports.DailyNote;

namespace TimeTracker.Tests.Features.Reports;

/// <summary>
/// Tests for ReportsHandler.PushDailyNoteAsync — uses a real temp directory for file I/O.
/// </summary>
public class ReportsHandlerPushTests : IDisposable
{
    private readonly string _tempDir;

    public ReportsHandlerPushTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"TimeTrackerTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private UserSettings SettingsFor(string subfolder = "Journal\\Daily") =>
        new() { Id = 1, VaultRootPath = _tempDir, DailyNotesSubfolder = subfolder };

    private static ReportsHandler CreateHandler()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<TimeTracker.Web.Data.AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReportsHandler(new TimeTracker.Web.Data.AppDbContext(options));
    }

    [Fact]
    public async Task PushDailyNoteAsync_VaultPathNotConfigured_ThrowsInvalidOperationException()
    {
        var handler = CreateHandler();
        var settings = new UserSettings { Id = 1, VaultRootPath = null };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.PushDailyNoteAsync(new DateOnly(2026, 3, 9), "# content", settings));
    }

    [Fact]
    public async Task PushDailyNoteAsync_EmptyVaultPath_ThrowsInvalidOperationException()
    {
        var handler = CreateHandler();
        var settings = new UserSettings { Id = 1, VaultRootPath = "   " };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.PushDailyNoteAsync(new DateOnly(2026, 3, 9), "# content", settings));
    }

    [Fact]
    public async Task PushDailyNoteAsync_NewFile_WritesFullMarkdown()
    {
        var handler = CreateHandler();
        var settings = SettingsFor();
        var markdown = "---\ndate: 2026-03-09\n---\n\n## ⏱ Time Tracker\n\nSome content";
        var date = new DateOnly(2026, 3, 9);

        var (filePath, appended) = await handler.PushDailyNoteAsync(date, markdown, settings);

        Assert.True(File.Exists(filePath));
        Assert.Equal(markdown, await File.ReadAllTextAsync(filePath));
        Assert.False(appended);
    }

    [Fact]
    public async Task PushDailyNoteAsync_NewFile_UsesCorrectFilename()
    {
        var handler = CreateHandler();
        var settings = SettingsFor();
        var date = new DateOnly(2026, 3, 9);

        var (filePath, _) = await handler.PushDailyNoteAsync(date, "# test", settings);

        Assert.EndsWith("2026-03-09.md", filePath);
    }

    [Fact]
    public async Task PushDailyNoteAsync_NewFile_CreatesSubfolderIfMissing()
    {
        var handler = CreateHandler();
        var subfolder = Path.Combine("Journal", "Daily");
        var settings = SettingsFor(subfolder);
        var date = new DateOnly(2026, 3, 9);

        await handler.PushDailyNoteAsync(date, "# test", settings);

        var expectedDir = Path.Combine(_tempDir, subfolder);
        Assert.True(Directory.Exists(expectedDir));
    }

    [Fact]
    public async Task PushDailyNoteAsync_ExistingFileWithSection_ReplacesSection()
    {
        var handler = CreateHandler();
        var settings = SettingsFor();
        var date = new DateOnly(2026, 3, 9);
        var folder = Path.Combine(_tempDir, "Journal\\Daily");
        Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder, "2026-03-09.md");

        // Create existing file with the section already present
        var existingContent = "# My Daily Note\n\nSome notes before.\n\n## ⏱ Time Tracker\n\nOLD CONTENT\n";
        await File.WriteAllTextAsync(filePath, existingContent);

        var newMarkdown = "---\ndate: 2026-03-09\n---\n\n## ⏱ Time Tracker\n\nNEW CONTENT\n";
        var (_, appended) = await handler.PushDailyNoteAsync(date, newMarkdown, settings);

        var result = await File.ReadAllTextAsync(filePath);
        Assert.Contains("NEW CONTENT", result);
        Assert.DoesNotContain("OLD CONTENT", result);
        Assert.Contains("Some notes before", result);
        Assert.True(appended);
    }

    [Fact]
    public async Task PushDailyNoteAsync_ExistingFileWithoutSection_AppendsSection()
    {
        var handler = CreateHandler();
        var settings = SettingsFor();
        var date = new DateOnly(2026, 3, 9);
        var folder = Path.Combine(_tempDir, "Journal\\Daily");
        Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder, "2026-03-09.md");

        // Create existing file WITHOUT the Time Tracker section
        var existingContent = "# My Daily Note\n\nSome existing content.\n";
        await File.WriteAllTextAsync(filePath, existingContent);

        var newMarkdown = "---\ndate: 2026-03-09\n---\n\n## ⏱ Time Tracker\n\nAPPENDED CONTENT\n";
        var (_, appended) = await handler.PushDailyNoteAsync(date, newMarkdown, settings);

        var result = await File.ReadAllTextAsync(filePath);
        Assert.Contains("Some existing content", result);
        Assert.Contains("APPENDED CONTENT", result);
        Assert.Contains("## ⏱ Time Tracker", result);
        Assert.True(appended);
    }

    [Fact]
    public async Task PushDailyNoteAsync_ExistingFileWithSection_DoesNotDuplicateFrontmatter()
    {
        var handler = CreateHandler();
        var settings = SettingsFor();
        var date = new DateOnly(2026, 3, 9);
        var folder = Path.Combine(_tempDir, "Journal\\Daily");
        Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder, "2026-03-09.md");

        await File.WriteAllTextAsync(filePath, "# Note\n\n## ⏱ Time Tracker\n\nOLD\n");

        var newMarkdown = "---\ndate: 2026-03-09\n---\n\n## ⏱ Time Tracker\n\nNEW\n";
        await handler.PushDailyNoteAsync(date, newMarkdown, settings);

        var result = await File.ReadAllTextAsync(filePath);
        // Frontmatter from new markdown should NOT appear — only section replaces in-place
        var frontmatterCount = result.Split("---").Length - 1;
        Assert.True(frontmatterCount < 2, "Frontmatter should not be duplicated when replacing a section.");
    }

    [Fact]
    public async Task PushDailyNoteAsync_ReturnsCorrectFilePath()
    {
        var handler = CreateHandler();
        var settings = SettingsFor();
        var date = new DateOnly(2026, 3, 15);

        var (filePath, _) = await handler.PushDailyNoteAsync(date, "# test", settings);

        var expectedPath = Path.Combine(_tempDir, "Journal\\Daily", "2026-03-15.md");
        Assert.Equal(expectedPath, filePath);
    }
}
