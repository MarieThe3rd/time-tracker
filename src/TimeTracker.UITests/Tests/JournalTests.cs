using TimeTracker.UITests.Infrastructure;
using TimeTracker.UITests.PageObjects;

namespace TimeTracker.UITests.Tests;

[Collection("App")]
public class JournalTests(AppFixture app)
{
    [Fact]
    public async Task JournalPage_PageLoads_ShowsHeading()
    {
        var page = await app.NewPageAsync();
        var journalPage = new JournalPage(page);
        await journalPage.GotoAsync();

        var heading = await journalPage.GetHeadingAsync();

        Assert.Contains("Journal", heading);
    }

    [Fact]
    public async Task JournalPage_ShowsTypeFilterDropdown()
    {
        var page = await app.NewPageAsync();
        var journalPage = new JournalPage(page);
        await journalPage.GotoAsync();

        await journalPage.TypeFilter.WaitForAsync();

        Assert.True(await journalPage.TypeFilter.IsVisibleAsync());
    }

    [Fact]
    public async Task JournalPage_TypeFilter_ContainsAllTypes()
    {
        var page = await app.NewPageAsync();
        var journalPage = new JournalPage(page);
        await journalPage.GotoAsync();

        var options = await journalPage.TypeFilter.Locator("option").AllInnerTextsAsync();

        Assert.Contains(options, o => o.Contains("Challenge"));
        Assert.Contains(options, o => o.Contains("Learning"));
        Assert.Contains(options, o => o.Contains("Success"));
    }

    [Fact]
    public async Task JournalPage_ShowsDateRangeInputs()
    {
        var page = await app.NewPageAsync();
        var journalPage = new JournalPage(page);
        await journalPage.GotoAsync();

        Assert.True(await journalPage.FromDateInput.IsVisibleAsync());
        Assert.True(await journalPage.ToDateInput.IsVisibleAsync());
    }

    [Fact]
    public async Task JournalPage_ShowsClearFiltersButton()
    {
        var page = await app.NewPageAsync();
        var journalPage = new JournalPage(page);
        await journalPage.GotoAsync();

        await journalPage.ClearFiltersButton.WaitForAsync();

        Assert.True(await journalPage.ClearFiltersButton.IsVisibleAsync());
    }

    [Fact]
    public async Task JournalPage_QuickAddFab_IsVisible()
    {
        var page = await app.NewPageAsync();
        var journalPage = new JournalPage(page);
        await journalPage.GotoAsync();

        await journalPage.QuickAddFab.WaitForAsync();

        Assert.True(await journalPage.QuickAddFab.IsVisibleAsync());
    }

    [Fact]
    public async Task JournalPage_QuickAddFab_Click_OpensPanel()
    {
        var page = await app.NewPageAsync();
        var journalPage = new JournalPage(page);
        await journalPage.GotoAsync();

        await journalPage.OpenQuickAddAsync();

        Assert.True(await journalPage.QuickAddPanel.IsVisibleAsync());
    }

    [Fact]
    public async Task JournalPage_QuickAddPanel_ShowsTitleInput()
    {
        var page = await app.NewPageAsync();
        var journalPage = new JournalPage(page);
        await journalPage.GotoAsync();
        await journalPage.OpenQuickAddAsync();

        Assert.True(await journalPage.TitleInput.IsVisibleAsync());
    }

    [Fact]
    public async Task JournalPage_AddSuccessEntry_AppearsInList()
    {
        var page = await app.NewPageAsync();
        var journalPage = new JournalPage(page);
        await journalPage.GotoAsync();

        var uniqueTitle = $"UI Test Win {Guid.NewGuid():N}";
        await journalPage.AddEntryAsync("Success", uniqueTitle, "Added via UI test");

        // Close the offcanvas panel using its dedicated close button
        await journalPage.QuickAddPanel.Locator(".btn-close").ClickAsync();
        await journalPage.WaitForBlazorAsync();

        // Journal page refreshes automatically after save; check entry appears
        var entryVisible = await page.Locator($"text={uniqueTitle}").IsVisibleAsync();
        Assert.True(entryVisible);
    }

    [Fact]
    public async Task JournalPage_AddChallengeEntry_ShowsSavedConfirmation()
    {
        var page = await app.NewPageAsync();
        var journalPage = new JournalPage(page);
        await journalPage.GotoAsync();

        await journalPage.AddEntryAsync("Challenge", $"UI Challenge {Guid.NewGuid():N}");

        Assert.True(await journalPage.SavedConfirmation.IsVisibleAsync());
    }

    [Fact]
    public async Task JournalPage_AddLearningEntry_ShowsSavedConfirmation()
    {
        var page = await app.NewPageAsync();
        var journalPage = new JournalPage(page);
        await journalPage.GotoAsync();

        await journalPage.AddEntryAsync("Learning", $"UI Learning {Guid.NewGuid():N}");

        Assert.True(await journalPage.SavedConfirmation.IsVisibleAsync());
    }

    [Fact]
    public async Task JournalPage_QuickAddPanel_FabVisibleOnDashboard()
    {
        // FAB is in MainLayout so it appears on every page
        var page = await app.NewPageAsync();
        await page.GotoAsync("/");
        await page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        var fab = page.Locator("button[title='Quick journal entry']");
        await fab.WaitForAsync();
        Assert.True(await fab.IsVisibleAsync());
    }
}
