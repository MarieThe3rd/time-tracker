using Microsoft.Playwright;
using TimeTracker.UITests.Infrastructure;

namespace TimeTracker.UITests.PageObjects;

/// <summary>
/// Page Object for the Reminders page (/reminders).
/// </summary>
public class RemindersPage(IPage page) : PageObjectBase(page)
{
    protected override string Route => "/reminders";

    // ── Header ────────────────────────────────────────────────────────────────

    public ILocator AddReminderButton => Page.Locator("#add-reminder-btn");

    // ── Show dismissed toggle ─────────────────────────────────────────────────

    public ILocator ShowDismissedToggle => Page.Locator("#showDismissed");

    // ── Reminder list ─────────────────────────────────────────────────────────

    public ILocator EmptyState => Page.GetByText("No reminders found.");

    public ILocator ReminderCards => Page.Locator("[data-testid='reminder-card']");

    public ILocator ReminderCardByTitle(string title) =>
        Page.Locator("[data-testid='reminder-card']", new() { HasText = title });

    public ILocator SnoozeButtonFor(string title) =>
        ReminderCardByTitle(title).Locator("[title='Snooze']");

    public ILocator DismissButtonFor(string title) =>
        ReminderCardByTitle(title).Locator("[title='Dismiss']");

    public ILocator EditButtonFor(string title) =>
        ReminderCardByTitle(title).Locator("[title='Edit']");

    public ILocator DeleteButtonFor(string title) =>
        ReminderCardByTitle(title).Locator("[title='Delete']");

    // ── Add / Edit modal ──────────────────────────────────────────────────────

    private ILocator FormModal =>
        Page.Locator(".modal.show",
            new() { Has = Page.Locator(".modal-title", new() { HasText = "Reminder" })
                            .Filter(new() { HasNotText = "Snooze" })
                            .Filter(new() { HasNotText = "Delete" }) });

    public ILocator ReminderTitleInput => Page.Locator("#reminder-title");
    public ILocator ReminderNotesInput => Page.Locator("#reminder-notes");
    public ILocator RemindOnInput      => Page.Locator("#reminder-remind-on");
    public ILocator RepeatSelect       => Page.Locator("#reminder-repeat");
    public ILocator StatusSelect       => Page.Locator("#reminder-status");
    public ILocator FormSaveButton     => FormModal.Locator("button[type='submit']");
    public ILocator FormCancelButton   => FormModal.Locator("button.btn-secondary");

    // ── Snooze modal ──────────────────────────────────────────────────────────

    private ILocator SnoozeModal =>
        Page.Locator(".modal.show",
            new() { Has = Page.Locator(".modal-title", new() { HasText = "Snooze Reminder" }) });

    public ILocator SnoozeTimeInput     => Page.Locator("#snooze-time-input");
    public ILocator SnoozeConfirmButton => Page.Locator("#snooze-confirm-btn");
    public ILocator SnoozeCancelButton  => SnoozeModal.Locator("button.btn-secondary");

    // ── Delete confirm modal ──────────────────────────────────────────────────

    private ILocator DeleteModal =>
        Page.Locator(".modal.show",
            new() { Has = Page.Locator(".modal-title", new() { HasText = "Delete Reminder" }) });

    public ILocator DeleteConfirmButton => Page.Locator("#delete-confirm-btn");
    public ILocator DeleteCancelButton  => DeleteModal.Locator("button.btn-secondary");

    // ── Helpers ───────────────────────────────────────────────────────────────

    public async Task OpenAddFormAsync()
    {
        await AddReminderButton.ClickAsync();
        await FormModal.WaitForAsync();
    }

    /// <summary>
    /// Fills the add/edit form and clicks Save.
    /// <paramref name="remindOn"/> must be a value accepted by datetime-local inputs (yyyy-MM-ddTHH:mm).
    /// </summary>
    public async Task FillAndSaveReminderAsync(
        string title,
        string remindOn,
        string? notes = null,
        string? repeat = null)
    {
        await ReminderTitleInput.FillAsync(title);
        await RemindOnInput.FillAsync(remindOn);

        if (notes is not null)
            await ReminderNotesInput.FillAsync(notes);

        if (repeat is not null)
            await RepeatSelect.SelectOptionAsync(new SelectOptionValue { Label = repeat });

        await FormSaveButton.ClickAsync();
        await WaitForBlazorAsync();
    }

    /// <summary>
    /// Opens the snooze modal for the given reminder, sets a new time, and confirms.
    /// <paramref name="newRemindOn"/> must be a value accepted by datetime-local inputs.
    /// </summary>
    public async Task SnoozeReminderAsync(string title, string newRemindOn)
    {
        await SnoozeButtonFor(title).ClickAsync();
        await SnoozeModal.WaitForAsync();
        await SnoozeTimeInput.FillAsync(newRemindOn);
        await SnoozeConfirmButton.ClickAsync();
        await WaitForBlazorAsync();
    }
}
