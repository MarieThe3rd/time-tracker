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
    public async Task ReportsPage_ClickAiInsightsTab_ShowsAiAnalyticsState()
    {
        var page = await app.NewPageAsync();
        var reportsPage = new ReportsPage(page);
        await reportsPage.GotoAsync();

        await reportsPage.AiInsightsTab.ClickAsync();
        await reportsPage.WaitForBlazorAsync();

        var cssClass = await reportsPage.AiInsightsTab.GetAttributeAsync("class");
        var showsEmptyState = await reportsPage.AiEmptyState.IsVisibleAsync();
        var showsWeeklySummary = await reportsPage.AiWeeklySummaryCard.IsVisibleAsync();
        var showsChart = await reportsPage.AiUsageChart.IsVisibleAsync();

        Assert.Contains("active", cssClass);
        Assert.True(showsEmptyState || showsWeeklySummary);
        Assert.True(showsEmptyState || showsChart);
    }

    [Fact]
    public async Task ReportsPage_ReviewExport_ShowsAiSummaryWhenAiEntriesExist()
    {
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

        var markdown = await reportsPage.MarkdownPreview.InnerTextAsync();

        Assert.Contains("### AI Usage Summary", markdown);
        Assert.Contains("**AI-assisted entries:**", markdown);
        Assert.Contains("Generated a tighter review summary", markdown);
        Assert.Contains("Used AI to structure follow-up notes.", markdown);
    }
}
