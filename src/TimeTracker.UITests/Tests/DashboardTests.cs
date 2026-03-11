using TimeTracker.UITests.Infrastructure;
using TimeTracker.UITests.PageObjects;

namespace TimeTracker.UITests.Tests;

[Collection("App")]
public class DashboardTests(AppFixture app)
{
    [Fact]
    public async Task Dashboard_PageLoads_ShowsHeading()
    {
        var page = await app.NewPageAsync();
        var dashboard = new DashboardPage(page);
        await dashboard.GotoAsync();

        var heading = await dashboard.GetHeadingAsync();

        Assert.Contains("Dashboard", heading);
    }

    [Fact]
    public async Task Dashboard_ShowsTimeTodayCard()
    {
        var page = await app.NewPageAsync();
        var dashboard = new DashboardPage(page);
        await dashboard.GotoAsync();

        await dashboard.TimeTodayCard.WaitForAsync();

        Assert.True(await dashboard.TimeTodayCard.IsVisibleAsync());
    }

    [Fact]
    public async Task Dashboard_ShowsAvgProductivityCard()
    {
        var page = await app.NewPageAsync();
        var dashboard = new DashboardPage(page);
        await dashboard.GotoAsync();

        await dashboard.AvgProductivityCard.WaitForAsync();

        Assert.True(await dashboard.AvgProductivityCard.IsVisibleAsync());
    }

    [Fact]
    public async Task Dashboard_ShowsEntriesTodayCard()
    {
        var page = await app.NewPageAsync();
        var dashboard = new DashboardPage(page);
        await dashboard.GotoAsync();

        await dashboard.EntriesTodayCard.WaitForAsync();

        Assert.True(await dashboard.EntriesTodayCard.IsVisibleAsync());
    }

    [Fact]
    public async Task Dashboard_ShowsAiTimeSavedCard()
    {
        var page = await app.NewPageAsync();
        var dashboard = new DashboardPage(page);
        await dashboard.GotoAsync();

        await dashboard.AiTimeSavedCard.WaitForAsync();

        Assert.True(await dashboard.AiTimeSavedCard.IsVisibleAsync());
    }

    [Fact]
    public async Task Dashboard_ShowsRecentEntriesSection()
    {
        var page = await app.NewPageAsync();
        var dashboard = new DashboardPage(page);
        await dashboard.GotoAsync();

        await dashboard.RecentEntriesCard.WaitForAsync();

        Assert.True(await dashboard.RecentEntriesCard.IsVisibleAsync());
    }

    [Fact]
    public async Task Dashboard_ShowsRecentJournalSection()
    {
        var page = await app.NewPageAsync();
        var dashboard = new DashboardPage(page);
        await dashboard.GotoAsync();

        await dashboard.RecentJournalCard.WaitForAsync();

        Assert.True(await dashboard.RecentJournalCard.IsVisibleAsync());
    }

    [Fact]
    public async Task Dashboard_ShowsTimeByCategorySection()
    {
        var page = await app.NewPageAsync();
        var dashboard = new DashboardPage(page);
        await dashboard.GotoAsync();

        await dashboard.TimeByCategoryCard.WaitForAsync();

        Assert.True(await dashboard.TimeByCategoryCard.IsVisibleAsync());
    }

    [Fact]
    public async Task Dashboard_Navigation_AllLinksVisible()
    {
        var page = await app.NewPageAsync();
        var dashboard = new DashboardPage(page);
        await dashboard.GotoAsync();

        Assert.True(await dashboard.NavTimer.IsVisibleAsync());
        Assert.True(await dashboard.NavJournal.IsVisibleAsync());
        Assert.True(await dashboard.NavReports.IsVisibleAsync());
        Assert.True(await dashboard.NavSettings.IsVisibleAsync());
    }

    [Fact]
    public async Task Dashboard_Navigation_DoesNotShowStandaloneAiUsageLink()
    {
        var page = await app.NewPageAsync();
        var dashboard = new DashboardPage(page);
        await dashboard.GotoAsync();

        Assert.Equal(0, await dashboard.NavAiUsage.CountAsync());
    }

    [Fact]
    public async Task Dashboard_Navigation_TimerLink_NavigatesToTimer()
    {
        var page = await app.NewPageAsync();
        var dashboard = new DashboardPage(page);
        await dashboard.GotoAsync();

        await dashboard.NavTimer.ClickAsync();
        await page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        Assert.Contains("/timer", page.Url);
    }
}
