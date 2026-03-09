using TimeTracker.UITests.Infrastructure;
using TimeTracker.UITests.PageObjects;

namespace TimeTracker.UITests.Tests;

[Collection("App")]
public class SettingsTests(AppFixture app)
{
    [Fact]
    public async Task SettingsPage_PageLoads_ShowsHeading()
    {
        var page = await app.NewPageAsync();
        var settingsPage = new SettingsPage(page);
        await settingsPage.GotoAsync();

        var heading = await settingsPage.GetHeadingAsync();

        Assert.Contains("Settings", heading);
    }

    [Fact]
    public async Task SettingsPage_ShowsObsidianVaultCard()
    {
        var page = await app.NewPageAsync();
        var settingsPage = new SettingsPage(page);
        await settingsPage.GotoAsync();

        await settingsPage.ObsidianCard.WaitForAsync();

        Assert.True(await settingsPage.ObsidianCard.IsVisibleAsync());
    }

    [Fact]
    public async Task SettingsPage_ShowsWorkCategoriesCard()
    {
        var page = await app.NewPageAsync();
        var settingsPage = new SettingsPage(page);
        await settingsPage.GotoAsync();

        await settingsPage.CategoriesCard.WaitForAsync();

        Assert.True(await settingsPage.CategoriesCard.IsVisibleAsync());
    }

    [Fact]
    public async Task SettingsPage_ShowsVaultPathInput()
    {
        var page = await app.NewPageAsync();
        var settingsPage = new SettingsPage(page);
        await settingsPage.GotoAsync();

        await settingsPage.VaultPathInput.WaitForAsync();

        Assert.True(await settingsPage.VaultPathInput.IsVisibleAsync());
    }

    [Fact]
    public async Task SettingsPage_ShowsDailyNotesSubfolderInput()
    {
        var page = await app.NewPageAsync();
        var settingsPage = new SettingsPage(page);
        await settingsPage.GotoAsync();

        await settingsPage.DailyNotesSubfolderInput.WaitForAsync();

        Assert.True(await settingsPage.DailyNotesSubfolderInput.IsVisibleAsync());
    }

    [Fact]
    public async Task SettingsPage_DailyNotesSubfolder_DefaultsToJournalDaily()
    {
        var page = await app.NewPageAsync();
        var settingsPage = new SettingsPage(page);
        await settingsPage.GotoAsync();

        var value = await settingsPage.DailyNotesSubfolderInput.InputValueAsync();

        Assert.Equal(@"Journal\Daily", value);
    }

    [Fact]
    public async Task SettingsPage_ShowsSaveButton()
    {
        var page = await app.NewPageAsync();
        var settingsPage = new SettingsPage(page);
        await settingsPage.GotoAsync();

        await settingsPage.SaveSettingsButton.WaitForAsync();

        Assert.True(await settingsPage.SaveSettingsButton.IsVisibleAsync());
    }

    [Fact]
    public async Task SettingsPage_CategoryList_ShowsSeededCategories()
    {
        var page = await app.NewPageAsync();
        var settingsPage = new SettingsPage(page);
        await settingsPage.GotoAsync();

        var categoryListText = await settingsPage.CategoryList.InnerTextAsync();

        Assert.Contains("Development", categoryListText);
    }

    [Fact]
    public async Task SettingsPage_CategoryList_SystemCategoriesShowSystemBadge()
    {
        var page = await app.NewPageAsync();
        var settingsPage = new SettingsPage(page);
        await settingsPage.GotoAsync();

        var systemBadges = page.Locator(".badge.bg-secondary", new() { HasText = "system" });
        var count = await systemBadges.CountAsync();

        Assert.True(count > 0, "Expected at least one system category badge.");
    }

    [Fact]
    public async Task SettingsPage_AddCategory_NewCategoryAppearsInList()
    {
        var page = await app.NewPageAsync();
        var settingsPage = new SettingsPage(page);
        await settingsPage.GotoAsync();

        var uniqueName = $"UITest {Guid.NewGuid():N}";
        await settingsPage.AddCategoryAsync(uniqueName[..25]); // trim to reasonable length

        var listText = await settingsPage.CategoryList.InnerTextAsync();
        Assert.Contains(uniqueName[..25], listText);
    }

    [Fact]
    public async Task SettingsPage_NewCategoryInput_IsVisible()
    {
        var page = await app.NewPageAsync();
        var settingsPage = new SettingsPage(page);
        await settingsPage.GotoAsync();

        await settingsPage.NewCategoryNameInput.WaitForAsync();

        Assert.True(await settingsPage.NewCategoryNameInput.IsVisibleAsync());
    }
}
