using Microsoft.Playwright;
using TimeTracker.UITests.Infrastructure;

namespace TimeTracker.UITests.PageObjects;

public class SettingsPage(IPage page) : PageObjectBase(page)
{
    protected override string Route => "/settings";

    public ILocator ObsidianCard => Page.Locator(".card-header", new() { HasText = "Obsidian Vault" });
    public ILocator CategoriesCard => Page.Locator(".card-header", new() { HasText = "Work Categories" });

    public ILocator VaultPathInput => Page.Locator("input[placeholder*='MyVault']");
    public ILocator DailyNotesSubfolderInput => Page.Locator("input.font-monospace").Last;
    public ILocator SaveSettingsButton => Page.Locator("button", new() { HasText = "Save" }).First;
    public ILocator SettingsSavedAlert => Page.Locator(".alert-success", new() { HasText = "Settings saved" });

    public ILocator NewCategoryNameInput => Page.Locator("input[placeholder='New category name']");
    // Add Category button: btn-sm btn-primary (FAB is rounded-circle, not btn-sm)
    public ILocator AddCategoryButton => Page.Locator("button.btn-sm.btn-primary");
    public ILocator CategoryList => Page.Locator(".list-group.list-group-flush");

    public async Task SaveVaultPathAsync(string path)
    {
        await VaultPathInput.FillAsync(path);
        await SaveSettingsButton.ClickAsync();
        await SettingsSavedAlert.WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    public async Task AddCategoryAsync(string name)
    {
        await NewCategoryNameInput.FillAsync(name);
        await NewCategoryNameInput.PressAsync("Tab"); // blur triggers @bind change event → _newCatName set in Blazor
        await Page.Keyboard.PressAsync("Tab");        // Tab from color picker to Add button
        await Page.Keyboard.PressAsync("Enter");      // activate button without pointer hit-test
        await WaitForBlazorAsync();
    }
}
