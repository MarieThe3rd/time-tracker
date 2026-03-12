using Microsoft.Playwright;
using TimeTracker.UITests.Infrastructure;

namespace TimeTracker.UITests.PageObjects;

public class TimerPage(IPage page) : PageObjectBase(page)
{
    protected override string Route => "/timer";

    public ILocator CategorySelect => Page.Locator("select.form-select").First;
    public ILocator DescriptionInput => Page.Locator("input[placeholder='What are you working on?']");
    public ILocator ElapsedDisplay => Page.Locator(".fs-4.fw-bold.font-monospace");
    public ILocator TimerStripElapsed => Page.Locator(".timer-strip span.font-monospace");

    // Use btn-lg to distinguish the page's Start/Stop from the timer-strip Stop (btn-sm)
    public ILocator StartButton => Page.Locator("button.btn-success.btn-lg");
    public ILocator StopButton => Page.Locator("button.btn-danger.btn-lg");

    public ILocator ManualEntryToggle => Page.Locator("button", new() { HasText = "Show" });
    public ILocator ManualEntryCard => Page.Locator(".card-header", new() { HasText = "Manual Entry" });

    // Scope SaveEntryButton to the manual-entry card to avoid matching QuickAdd panel's Save Entry
    private ILocator ManualEntrySection =>
        Page.Locator(".card", new() { Has = Page.Locator(".card-header", new() { HasText = "Manual Entry" }) });
    public ILocator SaveEntryButton => ManualEntrySection.Locator("button", new() { HasText = "Save Entry" });
    public ILocator ManualStartInput => ManualEntrySection.Locator("input[type='datetime-local']").First;
    public ILocator ManualEndInput => ManualEntrySection.Locator("input[type='datetime-local']").Nth(1);
    public ILocator ManualDescriptionInput => ManualEntrySection.Locator("label:has-text('Description') + input.form-control");
    public ILocator ManualValueAddedInput => ManualEntrySection.Locator("label:has-text('Value Added') + input.form-control");
    public ILocator ManualBreakCheckbox => ManualEntrySection.Locator("#manual-is-break");
    public ILocator ManualAiUsedCheckbox => ManualEntrySection.Locator("#manual-ai-used");
    public ILocator ManualAiTimeSavedInput => ManualEntrySection.Locator("label:has-text('Time Saved Using AI (minutes)') + input");
    public ILocator ManualAiNotesInput => ManualEntrySection.Locator("label:has-text('AI Notes') + textarea");
    public ILocator ManualCategorySelect => ManualEntrySection.Locator("select.form-select");

    public ILocator TodaysEntriesCard => Page.Locator(".card-header", new() { HasText = "Today's Entries" });
    public ILocator TimerStrip => Page.Locator(".timer-strip");
    public ILocator EditEntryCard => Page.Locator(".card", new() { Has = Page.Locator(".card-header", new() { HasText = "Edit Entry" }) });
    public ILocator EditStartInput => EditEntryCard.Locator("input[type='datetime-local']").First;
    public ILocator EditDescriptionInput => EditEntryCard.Locator("label:has-text('Description') + input.form-control");
    public ILocator EditCategorySelect => EditEntryCard.Locator("select.form-select");
    public ILocator SaveChangesButton => EditEntryCard.Locator("button", new() { HasText = "Save Changes" });

    public async Task StartTimerAsync(string? description = null)
    {
        if (description is not null)
            await DescriptionInput.FillAsync(description);
        await StartButton.ClickAsync();
        // Wait for the TimerStrip component (independent 1-second ticker) to appear.
        // This is more reliable than waiting for the page's Stop button, because
        // TimerStrip re-renders independently and just checks TimerService.IsRunning.
        await TimerStrip.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15_000 });
    }

    public async Task StopTimerAsync()
    {
        // Stop via the timer strip's own Stop button (btn-sm btn-danger) rather than
        // the page's large Stop button — the strip is always present when timer is running.
        var stripStop = TimerStrip.Locator("button.btn-sm.btn-danger");
        await stripStop.ClickAsync();
        // Wait for the strip to disappear (IsRunning = false propagates within 1 second)
        await TimerStrip.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 15_000 });
    }

    public async Task ShowManualEntryAsync()
    {
        await ManualEntryToggle.ClickAsync();
        await WaitForBlazorAsync();
    }

    public async Task SaveManualEntryAsync(
        string start,
        string end,
        string description,
        string? valueAdded,
        bool isBreak,
        bool aiUsed,
        int? aiTimeSavedMinutes,
        string? aiNotes)
    {
        await ManualStartInput.FillAsync(start);
        await ManualEndInput.FillAsync(end);
        await ManualDescriptionInput.FillAsync(description);
        await ManualValueAddedInput.FillAsync(valueAdded ?? string.Empty);

        if (isBreak != await ManualBreakCheckbox.IsCheckedAsync())
            await ManualBreakCheckbox.ClickAsync();

        if (aiUsed != await ManualAiUsedCheckbox.IsCheckedAsync())
            await ManualAiUsedCheckbox.ClickAsync();

        if (aiUsed)
        {
            await ManualAiTimeSavedInput.FillAsync((aiTimeSavedMinutes ?? 0).ToString());
            await ManualAiNotesInput.FillAsync(aiNotes ?? string.Empty);
        }

        await SaveEntryButton.ClickAsync();
        await WaitForBlazorAsync();
    }

    public async Task StartEditFirstEntryAsync()
    {
        var firstEditButton = Page.Locator("button.btn-outline-secondary", new() { Has = Page.Locator("i.bi-pencil") }).First;
        await firstEditButton.ClickAsync();
        await EditEntryCard.WaitForAsync();
    }

    public async Task StartEditEntryByDescriptionAsync(string description)
    {
        var row = Page.Locator("tbody tr", new() { HasText = description }).First;
        var editButton = row.Locator("button.btn-outline-secondary", new() { Has = Page.Locator("i.bi-pencil") });
        await editButton.ClickAsync();
        await EditEntryCard.WaitForAsync();
    }
}
