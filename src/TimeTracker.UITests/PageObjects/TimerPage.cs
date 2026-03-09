using Microsoft.Playwright;
using TimeTracker.UITests.Infrastructure;

namespace TimeTracker.UITests.PageObjects;

public class TimerPage(IPage page) : PageObjectBase(page)
{
    protected override string Route => "/timer";

    public ILocator CategorySelect => Page.Locator("select.form-select").First;
    public ILocator DescriptionInput => Page.Locator("input[placeholder='What are you working on?']");
    public ILocator ElapsedDisplay => Page.Locator(".display-6.fw-bold.font-monospace");

    // Use btn-lg to distinguish the page's Start/Stop from the timer-strip Stop (btn-sm)
    public ILocator StartButton => Page.Locator("button.btn-success.btn-lg");
    public ILocator StopButton => Page.Locator("button.btn-danger.btn-lg");

    public ILocator ManualEntryToggle => Page.Locator("button", new() { HasText = "Show" });
    public ILocator ManualEntryCard => Page.Locator(".card-header", new() { HasText = "Manual Entry" });

    // Scope SaveEntryButton to the manual-entry card to avoid matching QuickAdd panel's Save Entry
    private ILocator ManualEntrySection =>
        Page.Locator(".card", new() { Has = Page.Locator(".card-header", new() { HasText = "Manual Entry" }) });
    public ILocator SaveEntryButton => ManualEntrySection.Locator("button", new() { HasText = "Save Entry" });

    public ILocator TodaysEntriesCard => Page.Locator(".card-header", new() { HasText = "Today's Entries" });
    public ILocator TimerStrip => Page.Locator(".timer-strip");

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
}
