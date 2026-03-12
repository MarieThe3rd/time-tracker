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
    public async Task TimerPage_ShowManualEntry_DefaultsStartToNearestFifteenMinutes()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.ShowManualEntryAsync();
        await timerPage.ManualStartInput.WaitForAsync();

        var startValue = await timerPage.ManualStartInput.InputValueAsync();
        Assert.NotEmpty(startValue);

        var minutePortion = startValue[^2..];
        Assert.True(minutePortion is "00" or "15" or "30" or "45",
            $"Expected start minute to be a multiple of 15 (00, 15, 30, or 45), got '{minutePortion}'.");
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

    [Fact]
    public async Task TimerPage_SaveManualBreakWithAiDetails_DisplaysBreakAndAiUsage()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();
        await timerPage.ShowManualEntryAsync();

        var today = DateTime.Today;
        var start = today.AddHours(10).ToString("yyyy-MM-ddTHH:mm");
        var end = today.AddHours(10).AddMinutes(30).ToString("yyyy-MM-ddTHH:mm");

        await timerPage.SaveManualEntryAsync(
            start,
            end,
            "Break with AI notes",
            "Recovered focus quickly",
            isBreak: true,
            aiUsed: true,
            aiTimeSavedMinutes: 15,
            aiNotes: "Used AI summary for quick context switching.");

        Assert.True(await page.Locator("text=Break with AI notes").First.IsVisibleAsync());
        Assert.True(await page.Locator("text=Break").First.IsVisibleAsync());
        Assert.True(await page.Locator("text=Yes (15m)").First.IsVisibleAsync());
    }

    [Fact]
    public async Task TimerPage_EditEntry_UpdatesDateTimeDescriptionAndCategory()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();
        await timerPage.ShowManualEntryAsync();

        var today = DateTime.Today;
        var start = today.AddHours(11).ToString("yyyy-MM-ddTHH:mm");
        var end = today.AddHours(12).ToString("yyyy-MM-ddTHH:mm");

        await timerPage.SaveManualEntryAsync(
            start,
            end,
            "Entry before edit",
            "Initial value",
            isBreak: false,
            aiUsed: false,
            aiTimeSavedMinutes: null,
            aiNotes: null);

        await timerPage.StartEditEntryByDescriptionAsync("Entry before edit");

        var editedStart = today.AddHours(11).AddMinutes(30).ToString("yyyy-MM-ddTHH:mm");
        await timerPage.EditStartInput.FillAsync(editedStart);
        await timerPage.EditDescriptionInput.FillAsync("Entry after edit");
        await timerPage.EditCategorySelect.SelectOptionAsync(new[] { "2" });
        await timerPage.SaveChangesButton.ClickAsync();
        await timerPage.WaitForBlazorAsync();

        Assert.True(await page.Locator("text=Entry after edit").First.IsVisibleAsync());

        await timerPage.StartEditEntryByDescriptionAsync("Entry after edit");
        Assert.Equal("Entry after edit", await timerPage.EditDescriptionInput.InputValueAsync());
        Assert.Equal("2", await timerPage.EditCategorySelect.InputValueAsync());
    }

    // Wave 1 regression tests

    [Fact]
    public async Task TimerPage_ElapsedDisplay_DoesNotHaveDisplaySixClass()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.ElapsedDisplay.WaitForAsync();

        // Regression guard: display-6 class was removed in Wave 1 (now fs-4 fw-bold font-monospace)
        var oldStyleLocator = page.Locator(".display-6.font-monospace");
        Assert.Equal(0, await oldStyleLocator.CountAsync());

        // Confirm the new class is present
        var classes = await timerPage.ElapsedDisplay.GetAttributeAsync("class");
        Assert.Contains("fs-4", classes);
        Assert.Contains("font-monospace", classes);
    }

    [Fact]
    public async Task TimerPage_TimerStrip_ElapsedTimeHasFontMonoClass()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.StartTimerAsync("Strip font-mono test");

        await timerPage.TimerStripElapsed.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });
        var classes = await timerPage.TimerStripElapsed.GetAttributeAsync("class");
        Assert.Contains("font-monospace", classes);
        Assert.Contains("fs-6", classes);

        await timerPage.StopTimerAsync();
    }

    [Fact]
    public async Task TimerPage_ManualEntry_StartTimeIsPreFilled()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.ShowManualEntryAsync();
        await timerPage.ManualStartInput.WaitForAsync();

        var startValue = await timerPage.ManualStartInput.InputValueAsync();
        Assert.NotEmpty(startValue);
        Assert.Matches(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}", startValue);
    }

    [Fact]
    public async Task TimerPage_ManualEntry_EndTimeIsThirtyMinutesAfterStart()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.ShowManualEntryAsync();
        await timerPage.ManualStartInput.WaitForAsync();

        var startValue = await timerPage.ManualStartInput.InputValueAsync();
        var endValue = await timerPage.ManualEndInput.InputValueAsync();

        Assert.NotEmpty(startValue);
        Assert.NotEmpty(endValue);

        var start = DateTime.Parse(startValue);
        var end = DateTime.Parse(endValue);
        var diffMinutes = (end - start).TotalMinutes;

        Assert.InRange(diffMinutes, 25, 35);
    }
}
