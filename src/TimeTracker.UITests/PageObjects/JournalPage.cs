using Microsoft.Playwright;
using TimeTracker.UITests.Infrastructure;

namespace TimeTracker.UITests.PageObjects;

public class JournalPage(IPage page) : PageObjectBase(page)
{
    protected override string Route => "/journal";

    public ILocator TypeFilter => Page.Locator("select.form-select");
    // Date inputs — scoped to the filter card to avoid matching the QuickAdd panel's
    // hidden date input (always present in the DOM with visibility:hidden)
    private ILocator FilterCard => Page.Locator(".card").First;
    public ILocator FromDateInput => FilterCard.Locator("input[type='date']").First;
    public ILocator ToDateInput => FilterCard.Locator("input[type='date']").Last;
    public ILocator ClearFiltersButton => Page.Locator("button", new() { HasText = "Clear" });

    // FAB (floating action button) quick-add
    public ILocator QuickAddFab => Page.Locator("button[title='Quick journal entry']");

    // Quick-add offcanvas panel
    public ILocator QuickAddPanel => Page.Locator(".offcanvas[aria-label='Quick Journal Entry']");
    public ILocator TitleInput => Page.Locator("input[placeholder='Brief title...']");
    public ILocator NotesTextarea => Page.Locator("textarea[placeholder='Details...']");
    public ILocator SaveEntryButton => Page.Locator("button", new() { HasText = "Save Entry" });
    public ILocator SavedConfirmation => Page.Locator(".alert-success", new() { HasText = "✓ Saved!" });

    public ILocator TypeButton(string typeName) =>
        QuickAddPanel.Locator("button", new() { HasText = typeName });

    /// <summary>Opens the quick-add offcanvas and waits for it to appear.</summary>
    public async Task OpenQuickAddAsync()
    {
        await QuickAddFab.ClickAsync();
        await QuickAddPanel.WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    /// <summary>Fills and saves a journal entry via the quick-add panel.</summary>
    public async Task AddEntryAsync(string type, string title, string notes = "")
    {
        await OpenQuickAddAsync();
        await TypeButton(type).ClickAsync();
        await TitleInput.FillAsync(title);
        if (!string.IsNullOrEmpty(notes))
            await NotesTextarea.FillAsync(notes);
        await SaveEntryButton.ClickAsync();
        await SavedConfirmation.WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }
}
