using TimeTracker.UITests.Infrastructure;
using TimeTracker.UITests.PageObjects;

namespace TimeTracker.UITests.Tests;

[Collection("App")]
public class AiUsageReportTests(AppFixture app)
{
  [Fact]
  public async Task AiUsageReportPage_Loads_ShowsHeading_AndDateInputs()
  {
    var page = await app.NewPageAsync();
    var aiPage = new AiUsageReportPage(page);

    await aiPage.GotoAsync();

    var heading = await aiPage.GetHeadingAsync();

    Assert.Contains("AI Usage Report", heading);
    Assert.True(await aiPage.FromDateInput.IsVisibleAsync());
    Assert.True(await aiPage.ToDateInput.IsVisibleAsync());
  }

  [Fact]
  public async Task AiUsageReportPage_ShowsEitherEmptyStateOrReportData()
  {
    var page = await app.NewPageAsync();
    var aiPage = new AiUsageReportPage(page);

    await aiPage.GotoAsync();

    var showsEmptyState = await aiPage.EmptyState.IsVisibleAsync();
    var showsTable = await aiPage.DetailsTable.IsVisibleAsync();
    var showsChart = await aiPage.ChartCanvas.IsVisibleAsync();

    Assert.True(showsEmptyState || showsTable);
    Assert.True(showsEmptyState || showsChart);
  }

  [Fact]
  public async Task AiUsageReportPage_SidebarLink_NavigatesToAiUsageRoute()
  {
    var page = await app.NewPageAsync();

    await page.GotoAsync("/");
    await page.Locator("h4").First.WaitForAsync();

    var aiPage = new AiUsageReportPage(page);
    await aiPage.SidebarLink.ClickAsync();
    await aiPage.WaitForBlazorAsync();

    Assert.Contains("/reports/ai-usage", page.Url);
    Assert.Contains("AI Usage Report", await aiPage.GetHeadingAsync());
  }
}
