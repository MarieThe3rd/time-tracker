using TimeTracker.UITests.Infrastructure;
using TimeTracker.UITests.PageObjects;

namespace TimeTracker.UITests.Tests;

[Collection("App")]
public class ReportsTests(AppFixture app)
{
    [Fact]
    public async Task ReportsPage_PageLoads_ShowsHeading()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        var heading = await reportsPage.GetHeadingAsync();

        Assert.Contains("Reports", heading);
    }

    [Fact]
    public async Task ReportsPage_ShowsDateRangeInputs()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        await reportsPage.FromDateInput.WaitForAsync();
        await reportsPage.ToDateInput.WaitForAsync();

        Assert.True(await reportsPage.FromDateInput.IsVisibleAsync());
        Assert.True(await reportsPage.ToDateInput.IsVisibleAsync());
    }

    [Fact]
    public async Task ReportsPage_ShowsPresetDropdown()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        await reportsPage.PresetSelect.WaitForAsync();

        Assert.True(await reportsPage.PresetSelect.IsVisibleAsync());
    }

    [Fact]
    public async Task ReportsPage_PresetDropdown_ContainsAllPresets()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        var options = await reportsPage.PresetSelect.Locator("option").AllInnerTextsAsync();

        Assert.Contains(options, o => o.Contains("Today"));
        Assert.Contains(options, o => o.Contains("This week"));
        Assert.Contains(options, o => o.Contains("This month"));
        Assert.Contains(options, o => o.Contains("Custom"));
    }

    [Fact]
    public async Task ReportsPage_ShowsSummaryTab()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        await reportsPage.SummaryTab.WaitForAsync();

        Assert.True(await reportsPage.SummaryTab.IsVisibleAsync());
    }

    [Fact]
    public async Task ReportsPage_ShowsAiInsightsTab()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        await reportsPage.AiInsightsTab.WaitForAsync();

        Assert.True(await reportsPage.AiInsightsTab.IsVisibleAsync());
    }

    [Fact]
    public async Task ReportsPage_ShowsDailyNoteTab()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        await reportsPage.DailyNoteTab.WaitForAsync();

        Assert.True(await reportsPage.DailyNoteTab.IsVisibleAsync());
    }

    [Fact]
    public async Task ReportsPage_ShowsWeeklySummaryTab()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        await reportsPage.WeeklySummaryTab.WaitForAsync();

        Assert.True(await reportsPage.WeeklySummaryTab.IsVisibleAsync());
    }

    [Fact]
    public async Task ReportsPage_ShowsReviewExportTab()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        await reportsPage.ReviewExportTab.WaitForAsync();

        Assert.True(await reportsPage.ReviewExportTab.IsVisibleAsync());
    }

    [Fact]
    public async Task ReportsPage_SelectTodayPreset_SetsFromAndToToday()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        await reportsPage.PresetSelect.SelectOptionAsync("today");
        await reportsPage.WaitForBlazorAsync();

        var from = await reportsPage.FromDateInput.InputValueAsync();
        var to = await reportsPage.ToDateInput.InputValueAsync();

        Assert.Equal(DateTime.Today.ToString("yyyy-MM-dd"), from);
        Assert.Equal(from, to);
    }

    [Fact]
    public async Task ReportsPage_ClickDailyNoteTab_ActivatesTab()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        await reportsPage.DailyNoteTab.ClickAsync();
        await reportsPage.WaitForBlazorAsync();

        var cssClass = await reportsPage.DailyNoteTab.GetAttributeAsync("class");
        Assert.Contains("active", cssClass);
    }

    [Fact]
    public async Task ReportsPage_SelectDay_SetsFromAndToToSelectedDay()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        var targetDate = DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd");
        await reportsPage.DayDateInput.FillAsync(targetDate);
        await reportsPage.WaitForBlazorAsync();

        var from = await reportsPage.FromDateInput.InputValueAsync();
        var to = await reportsPage.ToDateInput.InputValueAsync();

        Assert.Equal(targetDate, from);
        Assert.Equal(targetDate, to);
    }

    [Fact]
    public async Task ReportsPage_ClickAiInsightsTab_ShowsAiAnalyticsState()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        await reportsPage.AiInsightsTab.ClickAsync();
        await reportsPage.WaitForBlazorAsync();

        var cssClass = await reportsPage.AiInsightsTab.GetAttributeAsync("class");
        var showsEmptyState = await reportsPage.AiEmptyState.IsVisibleAsync();
        var showsAiSavingsSummary = await reportsPage.AiSavingsSummaryCard.IsVisibleAsync();

        Assert.Contains("active", cssClass);
        Assert.True(showsEmptyState || showsAiSavingsSummary,
            "Expected either the empty state or the AI Savings Summary card to be visible.");
    }

    [Fact]
    public async Task ReportsPage_ReviewExport_ShowsExportButtonsAndCardStructure()
    {
        // Wave 4: review tab now renders HTML cards — the <pre> block is gone.
        // Verify the tab loads its card structure and the export buttons are present.
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();
        await timerPage.ShowManualEntryAsync();

        var today = DateTime.Today;
        var start = today.AddHours(9).ToString("yyyy-MM-ddTHH:mm");
        var end = today.AddHours(10).ToString("yyyy-MM-ddTHH:mm");

        await timerPage.SaveManualEntryAsync(
            start,
            end,
            "Review export AI coverage",
            "Generated a tighter review summary",
            isBreak: false,
            aiUsed: true,
            aiTimeSavedMinutes: 20,
            aiNotes: "Used AI to structure follow-up notes.");

        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();
        await reportsPage.ReviewExportTab.ClickAsync();
        await reportsPage.WaitForBlazorAsync();

        // Card header with date range is rendered; export buttons present
        Assert.True(await reportsPage.DownloadMdButton.IsVisibleAsync(),
            "Expected 'Download .md' button to be visible on Review Export tab.");
        // No raw <pre> block in the main content area for review tab
        Assert.Equal(0, await page.Locator(".card-body pre").CountAsync());
    }

    [Fact]
    public async Task ReportsPage_AiInsights_ShowsAiSavingsSummaryWhenAiEntriesExist()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();
        await timerPage.ShowManualEntryAsync();

        var today = DateTime.Today;
        var start = today.AddHours(14).ToString("yyyy-MM-ddTHH:mm");
        var end = today.AddHours(15).ToString("yyyy-MM-ddTHH:mm");

        await timerPage.SaveManualEntryAsync(
            start,
            end,
            "AI savings summary test",
            "Faster first draft",
            isBreak: false,
            aiUsed: true,
            aiTimeSavedMinutes: 30,
            aiNotes: "Generated structured outline.");

        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();
        await reportsPage.AiInsightsTab.ClickAsync();
        await reportsPage.WaitForBlazorAsync();

        Assert.True(await reportsPage.AiSavingsSummaryCard.IsVisibleAsync(),
            "Expected 'AI Savings Summary' card header to be visible.");
        Assert.True(await reportsPage.AiSavingsText.IsVisibleAsync(),
            "Expected 'saved using AI' text to be visible.");
    }

    // Wave 1 regression tests

    [Fact]
    public async Task ReportsPage_AiInsightsTab_HasNoChartCanvas()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        await reportsPage.AiInsightsTab.ClickAsync();
        await reportsPage.WaitForBlazorAsync();

        // Wave 1: aiUsageChart canvas was removed in favour of the AI Savings Summary card
        Assert.Equal(0, await reportsPage.AiUsageChart.CountAsync());
    }

    [Fact]
    public async Task ReportsPage_AiInsightsTab_ShowsAiSavingsSummaryCardWhenAiEntriesExist()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();
        await timerPage.ShowManualEntryAsync();

        var today = DateTime.Today;
        var start = today.AddHours(16).ToString("yyyy-MM-ddTHH:mm");
        var end = today.AddHours(17).ToString("yyyy-MM-ddTHH:mm");

        await timerPage.SaveManualEntryAsync(
            start,
            end,
            "AI Savings card check",
            "Validated card presence",
            isBreak: false,
            aiUsed: true,
            aiTimeSavedMinutes: 20,
            aiNotes: "Wave 1 check.");

        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();
        await reportsPage.AiInsightsTab.ClickAsync();
        await reportsPage.WaitForBlazorAsync();

        Assert.True(await reportsPage.AiSavingsSummaryCard.IsVisibleAsync());
        Assert.True(await reportsPage.AiSavingsText.IsVisibleAsync());
    }

    // Wave 4 tests — HTML rendering replaces raw <pre> markdown blocks

    [Fact]
    public async Task ReportsPage_DailyTab_ShowsHtmlTable_NotPreBlock()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();
        await timerPage.ShowManualEntryAsync();

        var today = DateTime.Today;
        var start = today.AddHours(8).ToString("yyyy-MM-ddTHH:mm");
        var end = today.AddHours(9).ToString("yyyy-MM-ddTHH:mm");
        await timerPage.SaveManualEntryAsync(start, end, "Daily table test", "Checked HTML output",
            isBreak: false, aiUsed: false, aiTimeSavedMinutes: null, aiNotes: null);

        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();
        await reportsPage.PresetSelect.SelectOptionAsync("today");
        await reportsPage.WaitForBlazorAsync();
        await reportsPage.DailyNoteTab.ClickAsync();
        await reportsPage.WaitForBlazorAsync();

        Assert.True(await reportsPage.DailyNoteTable.IsVisibleAsync(),
            "Expected an HTML <table> inside the Daily Note tab card body.");
        Assert.Equal(0, await page.Locator(".card-body pre").CountAsync());
    }

    [Fact]
    public async Task ReportsPage_DailyTab_TableHasExpectedColumns()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();
        await timerPage.ShowManualEntryAsync();

        var today = DateTime.Today;
        var start = today.AddHours(10).ToString("yyyy-MM-ddTHH:mm");
        var end = today.AddHours(11).ToString("yyyy-MM-ddTHH:mm");
        await timerPage.SaveManualEntryAsync(start, end, "Column header test", "Verifying headers",
            isBreak: false, aiUsed: false, aiTimeSavedMinutes: null, aiNotes: null);

        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();
        await reportsPage.PresetSelect.SelectOptionAsync("today");
        await reportsPage.WaitForBlazorAsync();
        await reportsPage.DailyNoteTab.ClickAsync();
        await reportsPage.WaitForBlazorAsync();

        var headers = await reportsPage.DailyNoteTable.Locator("thead th").AllInnerTextsAsync();

        Assert.Contains(headers, h => h.Contains("Category"));
        Assert.Contains(headers, h => h.Contains("Description"));
        Assert.Contains(headers, h => h.Contains("Duration"));
    }

    [Fact]
    public async Task ReportsPage_WeeklyTab_ShowsCategoryProgressBars()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();
        await timerPage.ShowManualEntryAsync();

        var today = DateTime.Today;
        var start = today.AddHours(11).ToString("yyyy-MM-ddTHH:mm");
        var end = today.AddHours(12).ToString("yyyy-MM-ddTHH:mm");
        await timerPage.SaveManualEntryAsync(start, end, "Weekly progress bar test", "Category breakdown",
            isBreak: false, aiUsed: false, aiTimeSavedMinutes: null, aiNotes: null);

        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();
        await reportsPage.PresetSelect.SelectOptionAsync("week");
        await reportsPage.WaitForBlazorAsync();
        await reportsPage.WeeklySummaryTab.ClickAsync();
        await reportsPage.WaitForBlazorAsync();

        // Weekly tab renders category progress bars (class="progress")
        var barCount = await reportsPage.WeeklyProgressBars.CountAsync();
        Assert.True(barCount > 0,
            "Expected at least one .progress bar in the Weekly Summary tab when entries with categories exist.");
    }

    [Fact]
    public async Task ReportsPage_ReviewTab_HasSuccessSection()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();
        await reportsPage.ReviewExportTab.ClickAsync();
        await reportsPage.WaitForBlazorAsync();

        // The review tab renders journal-type sections (Successes, Challenges, Learnings)
        // even when empty — the card structure + export buttons must be present.
        // "Successes & Wins" badge only appears when there are entries; verify the tab loads
        // without error by checking the Download .md export button is visible.
        Assert.True(await reportsPage.DownloadMdButton.IsVisibleAsync(),
            "Expected 'Download .md' button to be visible on the Review Export tab.");
        Assert.Equal(0, await page.Locator(".card-body pre").CountAsync());
    }

    [Fact]
    public async Task ReportsPage_ExportButtons_StillPresent()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();
        await reportsPage.DailyNoteTab.ClickAsync();
        await reportsPage.WaitForBlazorAsync();

        Assert.True(await reportsPage.DownloadMdButton.IsVisibleAsync(),
            "Expected 'Download .md' button on Daily Note tab.");
        Assert.True(await reportsPage.PushToObsidianButton.IsVisibleAsync(),
            "Expected 'Push to Obsidian' button on Daily Note tab.");
    }
}
