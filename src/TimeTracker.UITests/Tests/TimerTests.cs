using TimeTracker.UITests.Infrastructure;
using TimeTracker.UITests.PageObjects;

namespace TimeTracker.UITests.Tests;

[Collection("App")]
public class TimerTests(AppFixture app)
{
    [Fact]
    public async Task TimerPage_PageLoads_ShowsHeading()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        var heading = await timerPage.GetHeadingAsync();

        Assert.Contains("Timer", heading);
    }

    [Fact]
    public async Task TimerPage_ShowsCategoryDropdown()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.CategorySelect.WaitForAsync();

        Assert.True(await timerPage.CategorySelect.IsVisibleAsync());
    }

    [Fact]
    public async Task TimerPage_CategoryDropdown_ContainsSeededCategories()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        var options = await timerPage.CategorySelect.Locator("option").AllInnerTextsAsync();

        Assert.Contains(options, o => o.Contains("Development"));
    }

    [Fact]
    public async Task TimerPage_ShowsDescriptionInput()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.DescriptionInput.WaitForAsync();

        Assert.True(await timerPage.DescriptionInput.IsVisibleAsync());
    }

    [Fact]
    public async Task TimerPage_ShowsStartButton_WhenNotRunning()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.StartButton.WaitForAsync();

        Assert.True(await timerPage.StartButton.IsVisibleAsync());
    }

    [Fact]
    public async Task TimerPage_ShowsElapsedDisplay()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.ElapsedDisplay.WaitForAsync();

        Assert.True(await timerPage.ElapsedDisplay.IsVisibleAsync());
    }

    [Fact]
    public async Task TimerPage_StartTimer_ShowsStopButton()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.StartTimerAsync("UI Test timer start");

        // Timer strip appears when timer is running; its Stop button is the reliable signal
        Assert.True(await timerPage.TimerStrip.IsVisibleAsync());

        // Cleanup
        await timerPage.StopTimerAsync();
    }

    [Fact]
    public async Task TimerPage_StartTimer_ShowsTimerStrip()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.StartTimerAsync("Timer strip test");

        await timerPage.TimerStrip.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });
        Assert.True(await timerPage.TimerStrip.IsVisibleAsync());

        // Cleanup
        await timerPage.StopTimerAsync();
    }

    [Fact]
    public async Task TimerPage_StopTimer_HidesTimerStrip()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.StartTimerAsync("Stop test");
        await timerPage.StopTimerAsync();

        // Timer strip should be gone
        Assert.False(await timerPage.TimerStrip.IsVisibleAsync());
    }

    [Fact]
    public async Task TimerPage_StopTimer_EntryAppearsInTodaysList()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.DescriptionInput.FillAsync("Today list entry test");
        await timerPage.StartTimerAsync();
        await Task.Delay(500); // brief pause so duration > 0
        await timerPage.StopTimerAsync();

        // The today's entries table should appear (no longer "No entries yet today")
        var noEntries = page.Locator("text=No entries yet today.");
        Assert.False(await noEntries.IsVisibleAsync());
    }

    [Fact]
    public async Task TimerPage_ShowManualEntry_ShowsForm()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.ShowManualEntryAsync();

        await timerPage.SaveEntryButton.WaitForAsync();
        Assert.True(await timerPage.SaveEntryButton.IsVisibleAsync());
    }

    [Fact]
    public async Task TimerPage_ShowsTodaysEntriesCard()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.TodaysEntriesCard.WaitForAsync();

        Assert.True(await timerPage.TodaysEntriesCard.IsVisibleAsync());
    }
}
