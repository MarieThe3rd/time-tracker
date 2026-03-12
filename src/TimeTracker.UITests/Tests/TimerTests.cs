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

        // TimePicker uses a date input + time select; verify the select is pre-filled
        // with a time on a 15-minute boundary (slot.MinuteOfDay % 15 == 0)
        var dateValue = await timerPage.ManualStartInput.InputValueAsync();
        Assert.NotEmpty(dateValue);

        var timeSelectValue = await timerPage.ManualStartTimeSelect.InputValueAsync();
        Assert.NotEmpty(timeSelectValue);
        var minuteOfDay = int.Parse(timeSelectValue);
        Assert.True(minuteOfDay % 15 == 0,
            $"Expected start time to be on a 15-minute boundary, got {minuteOfDay} minutes.");
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

        var editedStartDt = today.AddHours(11).AddMinutes(30);
        await timerPage.EditStartInput.FillAsync(editedStartDt.ToString("yyyy-MM-dd"));
        await timerPage.EditStartTimeSelect.SelectOptionAsync(
            (editedStartDt.Hour * 60 + editedStartDt.Minute).ToString()); // 690 = 11:30
        await timerPage.EditDescriptionInput.FillAsync("Entry after edit");
        await timerPage.EditCategorySelect.SelectOptionAsync(new[] { "2" });
        await timerPage.SaveChangesButton.ClickAsync();
        await timerPage.WaitForBlazorAsync();

        Assert.True(await page.Locator("text=Entry after edit").First.IsVisibleAsync());

        await timerPage.StartEditEntryByDescriptionAsync("Entry after edit");
        Assert.Equal("Entry after edit", await timerPage.EditDescriptionInput.InputValueAsync());
        Assert.Equal("2", await timerPage.EditCategorySelect.InputValueAsync());
    }

    // Wave 2: TimePicker component tests

    [Fact]
    public async Task TimerPage_ManualEntry_StartTimeUsesSelectNotDatetimeLocal()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.ShowManualEntryAsync();
        await timerPage.ManualStartInput.WaitForAsync();

        // TimePicker replaced datetime-local; verify the old input type is gone
        var datetimeLocalCount = await timerPage.ManualEntrySection
            .Locator("input[type='datetime-local']").CountAsync();
        Assert.Equal(0, datetimeLocalCount);

        // And the time select is present
        var timeSelectCount = await timerPage.ManualEntrySection
            .Locator("select.form-select-sm").CountAsync();
        Assert.True(timeSelectCount > 0,
            "Expected at least one time select (form-select-sm) in the manual entry form.");
    }

    [Fact]
    public async Task TimerPage_ManualEntry_TimeSelect_HasExpectedOptions()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.ShowManualEntryAsync();
        await timerPage.ManualStartTimeSelect.WaitForAsync();

        var options = await timerPage.ManualStartTimeSelect
            .Locator("option").AllInnerTextsAsync();

        // 96 quarter-hour slots + 1 placeholder = 97 total
        Assert.True(options.Count >= 96,
            $"Expected at least 96 time options, got {options.Count}.");
        Assert.Contains("12:00 AM", options);
        Assert.Contains("11:45 PM", options);
    }

    [Fact]
    public async Task TimerPage_ManualEntry_SelectingTimeSlot_UpdatesValue()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.ShowManualEntryAsync();
        await timerPage.ManualStartTimeSelect.WaitForAsync();

        // Select 2:00 PM = 14 * 60 = 840 minutes
        await timerPage.ManualStartTimeSelect.SelectOptionAsync("840");

        var selectedValue = await timerPage.ManualStartTimeSelect.InputValueAsync();
        Assert.Equal("840", selectedValue);
    }

    // Wave 2: Chronological entry list tests

    [Fact]
    public async Task TimerPage_Entries_ShowStartAndEndColumns()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        var headers = await page.Locator("thead th").AllInnerTextsAsync();

        Assert.Contains("Start", headers);
        Assert.Contains("End", headers);
    }

    [Fact]
    public async Task TimerPage_Entries_SortedChronologically()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        var today = DateTime.Today;

        // Add the later-time entry first to verify the list re-sorts it
        await timerPage.ShowManualEntryAsync();
        await timerPage.SaveManualEntryAsync(
            today.AddHours(16).ToString("yyyy-MM-ddTHH:mm"),
            today.AddHours(16).AddMinutes(30).ToString("yyyy-MM-ddTHH:mm"),
            "Chrono sort later", null, false, false, null, null);

        await timerPage.ShowManualEntryAsync();
        await timerPage.SaveManualEntryAsync(
            today.AddHours(7).ToString("yyyy-MM-ddTHH:mm"),
            today.AddHours(7).AddMinutes(30).ToString("yyyy-MM-ddTHH:mm"),
            "Chrono sort earlier", null, false, false, null, null);

        var entryRows = page.Locator("tbody tr:not(.table-warning)");
        var allRows = await entryRows.AllAsync();

        int earlierIndex = -1, laterIndex = -1;
        for (int i = 0; i < allRows.Count; i++)
        {
            var text = await allRows[i].InnerTextAsync();
            if (text.Contains("Chrono sort earlier")) earlierIndex = i;
            if (text.Contains("Chrono sort later"))   laterIndex   = i;
        }

        Assert.NotEqual(-1, earlierIndex);
        Assert.NotEqual(-1, laterIndex);
        Assert.True(earlierIndex < laterIndex,
            $"'Chrono sort earlier' (row {earlierIndex}) should appear before 'Chrono sort later' (row {laterIndex}).");
    }

    [Fact]
    public async Task TimerPage_Entries_ShowsGapRow_WhenGapExists()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        var today = DateTime.Today;

        // Entry 1: 8:00–8:30; Entry 2: 9:00–9:30 → 30-min gap triggers a table-warning row
        await timerPage.ShowManualEntryAsync();
        await timerPage.SaveManualEntryAsync(
            today.AddHours(8).ToString("yyyy-MM-ddTHH:mm"),
            today.AddHours(8).AddMinutes(30).ToString("yyyy-MM-ddTHH:mm"),
            "Gap row test A", null, false, false, null, null);

        await timerPage.ShowManualEntryAsync();
        await timerPage.SaveManualEntryAsync(
            today.AddHours(9).ToString("yyyy-MM-ddTHH:mm"),
            today.AddHours(9).AddMinutes(30).ToString("yyyy-MM-ddTHH:mm"),
            "Gap row test B", null, false, false, null, null);

        var gapRows = page.Locator("tbody tr.table-warning");
        Assert.True(await gapRows.CountAsync() > 0,
            "Expected at least one gap row (tr.table-warning) between entries with a gap > 2 minutes.");
    }

    [Fact]
    public async Task TimerPage_GapRow_AddEntryButton_PreFillsForm()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        var today = DateTime.Today;

        // Entry 1: 13:00–13:30 (1 PM–1:30 PM)
        await timerPage.ShowManualEntryAsync();
        await timerPage.SaveManualEntryAsync(
            today.AddHours(13).ToString("yyyy-MM-ddTHH:mm"),
            today.AddHours(13).AddMinutes(30).ToString("yyyy-MM-ddTHH:mm"),
            "Gap btn test A", null, false, false, null, null);

        // Entry 2: 14:00–14:30 (2 PM–2:30 PM) → 30-min gap
        await timerPage.ShowManualEntryAsync();
        await timerPage.SaveManualEntryAsync(
            today.AddHours(14).ToString("yyyy-MM-ddTHH:mm"),
            today.AddHours(14).AddMinutes(30).ToString("yyyy-MM-ddTHH:mm"),
            "Gap btn test B", null, false, false, null, null);

        // Find the gap row that sits between our two entries and click its Add Entry button
        var gapRow = page.Locator("tbody tr.table-warning",
            new() { HasText = "1:30 PM" }).First;
        var addEntryButton = gapRow.Locator("button", new() { HasText = "Add Entry" });
        await addEntryButton.ClickAsync();
        await timerPage.WaitForBlazorAsync();

        // Manual entry form should open with start pre-filled to gap start (13:30 = 810 min)
        await timerPage.SaveEntryButton.WaitForAsync();
        var startTimeValue = await timerPage.ManualStartTimeSelect.InputValueAsync();
        Assert.Equal("810", startTimeValue); // 13:30 = 13*60+30 = 810

        // End should be pre-filled to gap end (14:00 = 840 min)
        var endTimeValue = await timerPage.ManualEndTimeSelect.InputValueAsync();
        Assert.Equal("840", endTimeValue); // 14:00 = 14*60 = 840
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

        // TimePicker exposes a date input (yyyy-MM-dd) and a time select (minuteOfDay)
        var dateValue = await timerPage.ManualStartInput.InputValueAsync();
        Assert.NotEmpty(dateValue);
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", dateValue);
    }

    [Fact]
    public async Task TimerPage_ManualEntry_EndTimeIsThirtyMinutesAfterStart()
    {
        var page = await app.NewPageAsync();
        var timerPage = new TimerPage(page);
        await timerPage.GotoAsync();

        await timerPage.ShowManualEntryAsync();
        await timerPage.ManualStartInput.WaitForAsync();

        // TimePicker exposes a date input + a time select (value = minuteOfDay)
        var startDate = await timerPage.ManualStartInput.InputValueAsync();
        var startTime = await timerPage.ManualStartTimeSelect.InputValueAsync();
        var endDate   = await timerPage.ManualEndInput.InputValueAsync();
        var endTime   = await timerPage.ManualEndTimeSelect.InputValueAsync();

        Assert.NotEmpty(startDate);
        Assert.NotEmpty(startTime);
        Assert.NotEmpty(endDate);
        Assert.NotEmpty(endTime);

        var startDt = DateTime.Parse(startDate).AddMinutes(int.Parse(startTime));
        var endDt   = DateTime.Parse(endDate).AddMinutes(int.Parse(endTime));
        var diffMinutes = (endDt - startDt).TotalMinutes;

        Assert.InRange(diffMinutes, 25, 35);
    }
}
