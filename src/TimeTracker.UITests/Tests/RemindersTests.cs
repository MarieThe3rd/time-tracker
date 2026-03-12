using TimeTracker.UITests.Infrastructure;
using TimeTracker.UITests.PageObjects;

namespace TimeTracker.UITests.Tests;

[Collection("App")]
public class RemindersTests(AppFixture app)
{
    // Helper: a future datetime string valid for datetime-local inputs
    private static string FutureRemindOn(int hoursFromNow = 2) =>
        DateTime.Now.AddHours(hoursFromNow).ToString("yyyy-MM-ddTHH:mm");

    // ── Page load ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RemindersPage_PageLoads_ShowsHeading()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        var heading = await remindersPage.GetHeadingAsync();

        Assert.Contains("Reminders", heading);
    }

    [Fact]
    public async Task RemindersPage_PageLoads_ShowsAddReminderButton()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        await remindersPage.AddReminderButton.WaitForAsync();

        Assert.True(await remindersPage.AddReminderButton.IsVisibleAsync());
    }

    [Fact]
    public async Task RemindersPage_PageLoads_ShowsShowDismissedToggle()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        Assert.True(await remindersPage.ShowDismissedToggle.IsVisibleAsync());
    }

    /// <summary>
    /// Verifies the empty-state message when no active reminders exist.
    /// NOTE: This test assumes a clean UITest database. If reminders have been added by
    /// previous test runs the empty-state element will not be visible; the test adapts
    /// gracefully in that scenario.
    /// </summary>
    [Fact]
    public async Task RemindersPage_WhenNoReminders_ShowsEmptyState()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        var cardCount = await remindersPage.ReminderCards.CountAsync();
        if (cardCount == 0)
        {
            await remindersPage.EmptyState.WaitForAsync();
            Assert.True(await remindersPage.EmptyState.IsVisibleAsync());
        }
        else
        {
            Assert.False(await remindersPage.EmptyState.IsVisibleAsync(),
                "Empty-state message should not be visible when reminders are present.");
        }
    }

    // ── Add reminder ──────────────────────────────────────────────────────────

    [Fact]
    public async Task RemindersPage_AddReminder_ModalOpens_WithNewReminderTitle()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        await remindersPage.OpenAddFormAsync();

        Assert.True(await page.Locator("text=New Reminder").IsVisibleAsync(),
            "Expected 'New Reminder' heading in the add modal.");
    }

    [Fact]
    public async Task RemindersPage_AddReminder_ModalCancels_AndModalDisappears()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        await remindersPage.OpenAddFormAsync();
        await remindersPage.FormCancelButton.ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        Assert.Equal(0, await page.Locator(".modal.show").CountAsync());
    }

    [Fact]
    public async Task RemindersPage_AddReminder_HappyPath_CardAppearsInList()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        const string title = "UI Test — Add Reminder Happy Path";
        await remindersPage.OpenAddFormAsync();
        await remindersPage.FillAndSaveReminderAsync(
            title: title,
            remindOn: FutureRemindOn(),
            notes: "Created by Playwright");

        await remindersPage.ReminderCardByTitle(title).WaitForAsync();
        Assert.True(await remindersPage.ReminderCardByTitle(title).IsVisibleAsync());
    }

    [Fact]
    public async Task RemindersPage_AddReminder_MissingTitle_ShowsValidationError()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        await remindersPage.OpenAddFormAsync();
        // Submit without filling the required Title field
        await remindersPage.FormSaveButton.ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        Assert.True(await page.Locator("text=Title is required.").IsVisibleAsync(),
            "Expected 'Title is required.' validation message.");
        Assert.Equal(1, await page.Locator(".modal.show").CountAsync());
    }

    [Fact]
    public async Task RemindersPage_AddReminder_WithRepeat_ShowsRepeatBadge()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        const string title = "UI Test — Add Reminder Daily";
        await remindersPage.OpenAddFormAsync();
        await remindersPage.FillAndSaveReminderAsync(
            title: title,
            remindOn: FutureRemindOn(),
            repeat: "Daily");

        var card = remindersPage.ReminderCardByTitle(title);
        await card.WaitForAsync();
        Assert.True(await card.Locator("text=Daily").IsVisibleAsync(),
            "Expected 'Daily' repeat badge on the reminder card.");
    }

    // ── Snooze ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RemindersPage_SnoozeReminder_SnoozeModalOpens()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        const string title = "UI Test — Snooze Modal Opens";
        await remindersPage.OpenAddFormAsync();
        await remindersPage.FillAndSaveReminderAsync(title: title, remindOn: FutureRemindOn());

        await remindersPage.SnoozeButtonFor(title).ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        Assert.True(await remindersPage.SnoozeTimeInput.IsVisibleAsync(),
            "Expected snooze datetime input to be visible in the snooze modal.");
        Assert.True(await remindersPage.SnoozeConfirmButton.IsVisibleAsync());
    }

    [Fact]
    public async Task RemindersPage_SnoozeReminder_CancelLeavesReminderUnchanged()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        const string title = "UI Test — Snooze Cancel";
        await remindersPage.OpenAddFormAsync();
        await remindersPage.FillAndSaveReminderAsync(title: title, remindOn: FutureRemindOn());

        await remindersPage.SnoozeButtonFor(title).ClickAsync();
        await remindersPage.WaitForBlazorAsync();
        await remindersPage.SnoozeCancelButton.ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        // Modal closed, reminder still visible
        Assert.Equal(0, await page.Locator(".modal.show").CountAsync());
        Assert.True(await remindersPage.ReminderCardByTitle(title).IsVisibleAsync());
    }

    [Fact]
    public async Task RemindersPage_SnoozeReminder_ConfirmUpdatesStatus()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        const string title = "UI Test — Snooze Confirm";
        await remindersPage.OpenAddFormAsync();
        await remindersPage.FillAndSaveReminderAsync(title: title, remindOn: FutureRemindOn(1));

        // Snooze to 4 hours from now
        await remindersPage.SnoozeReminderAsync(title, newRemindOn: FutureRemindOn(4));

        // Card should still be visible and status badge should show "Snoozed"
        var card = remindersPage.ReminderCardByTitle(title);
        await card.WaitForAsync();
        Assert.True(await card.Locator("text=Snoozed").IsVisibleAsync(),
            "Expected 'Snoozed' status badge after confirming snooze.");
    }

    // ── Dismiss ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task RemindersPage_DismissReminder_HidesItByDefault()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        const string title = "UI Test — Dismiss Reminder";
        await remindersPage.OpenAddFormAsync();
        await remindersPage.FillAndSaveReminderAsync(title: title, remindOn: FutureRemindOn());

        await remindersPage.DismissButtonFor(title).ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        // Without "Show dismissed" toggle, dismissed reminders are hidden
        Assert.Equal(0, await remindersPage.ReminderCardByTitle(title).CountAsync());
    }

    [Fact]
    public async Task RemindersPage_ShowDismissedToggle_RevealsDismissedReminders()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        const string title = "UI Test — Show Dismissed Toggle";
        await remindersPage.OpenAddFormAsync();
        await remindersPage.FillAndSaveReminderAsync(title: title, remindOn: FutureRemindOn());

        // Dismiss it
        await remindersPage.DismissButtonFor(title).ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        // Should be gone from default view
        Assert.Equal(0, await remindersPage.ReminderCardByTitle(title).CountAsync());

        // Enable the toggle
        await remindersPage.ShowDismissedToggle.ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        // Now the dismissed reminder should be visible
        await remindersPage.ReminderCardByTitle(title).WaitForAsync();
        Assert.True(await remindersPage.ReminderCardByTitle(title).IsVisibleAsync(),
            "Expected dismissed reminder to be visible after enabling 'Show dismissed'.");
    }

    [Fact]
    public async Task RemindersPage_DismissedReminder_ShowsDismissedBadge()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        const string title = "UI Test — Dismissed Badge";
        await remindersPage.OpenAddFormAsync();
        await remindersPage.FillAndSaveReminderAsync(title: title, remindOn: FutureRemindOn());

        await remindersPage.DismissButtonFor(title).ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        // Enable show-dismissed then verify the status badge reads "Dismissed"
        await remindersPage.ShowDismissedToggle.ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        var card = remindersPage.ReminderCardByTitle(title);
        await card.WaitForAsync();
        Assert.True(await card.Locator("text=Dismissed").IsVisibleAsync(),
            "Expected 'Dismissed' status badge on a dismissed reminder.");
    }

    // ── Edit reminder ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RemindersPage_EditReminder_ModalOpens_WithEditReminderTitle()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        const string title = "UI Test — Edit Reminder Setup";
        await remindersPage.OpenAddFormAsync();
        await remindersPage.FillAndSaveReminderAsync(title: title, remindOn: FutureRemindOn());

        await remindersPage.EditButtonFor(title).ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        // The edit modal title reads "Edit Reminder"
        Assert.True(await page.Locator("text=Edit Reminder").IsVisibleAsync(),
            "Expected 'Edit Reminder' heading in the modal.");
    }

    [Fact]
    public async Task RemindersPage_EditReminder_UpdatesTitle_AndReflectsOnCard()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        const string original = "UI Test — Edit Reminder Original";
        const string updated  = "UI Test — Edit Reminder Updated";

        await remindersPage.OpenAddFormAsync();
        await remindersPage.FillAndSaveReminderAsync(title: original, remindOn: FutureRemindOn());

        await remindersPage.EditButtonFor(original).ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        await remindersPage.ReminderTitleInput.ClearAsync();
        await remindersPage.ReminderTitleInput.FillAsync(updated);
        await remindersPage.FormSaveButton.ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        Assert.True(await remindersPage.ReminderCardByTitle(updated).IsVisibleAsync(),
            "Expected updated reminder title to appear as a card.");
        Assert.Equal(0, await remindersPage.ReminderCardByTitle(original).CountAsync());
    }

    // ── Delete reminder ───────────────────────────────────────────────────────

    [Fact]
    public async Task RemindersPage_DeleteReminder_ConfirmDialog_Opens()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        const string title = "UI Test — Delete Reminder Dialog";
        await remindersPage.OpenAddFormAsync();
        await remindersPage.FillAndSaveReminderAsync(title: title, remindOn: FutureRemindOn());

        await remindersPage.DeleteButtonFor(title).ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        Assert.True(await page.Locator(".modal.show").IsVisibleAsync(),
            "Expected delete confirmation modal to appear.");
        Assert.True(await page.Locator("text=Delete Reminder").IsVisibleAsync());
    }

    [Fact]
    public async Task RemindersPage_DeleteReminder_CancelKeepsReminder()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        const string title = "UI Test — Delete Reminder Cancel";
        await remindersPage.OpenAddFormAsync();
        await remindersPage.FillAndSaveReminderAsync(title: title, remindOn: FutureRemindOn());

        await remindersPage.DeleteButtonFor(title).ClickAsync();
        await remindersPage.WaitForBlazorAsync();
        await remindersPage.DeleteCancelButton.ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        Assert.True(await remindersPage.ReminderCardByTitle(title).IsVisibleAsync(),
            "Expected reminder to remain after cancelling deletion.");
    }

    [Fact]
    public async Task RemindersPage_DeleteReminder_ConfirmRemovesReminder()
    {
        var page = await app.NewPageAsync();
        var remindersPage = new RemindersPage(page);
        await remindersPage.GotoAsync();

        const string title = "UI Test — Delete Reminder Confirm";
        await remindersPage.OpenAddFormAsync();
        await remindersPage.FillAndSaveReminderAsync(title: title, remindOn: FutureRemindOn());

        await remindersPage.DeleteButtonFor(title).ClickAsync();
        await remindersPage.WaitForBlazorAsync();
        await remindersPage.DeleteConfirmButton.ClickAsync();
        await remindersPage.WaitForBlazorAsync();

        Assert.Equal(0, await remindersPage.ReminderCardByTitle(title).CountAsync());
    }
}
