using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;
using TimeTracker.UITests.PageObjects;

namespace TimeTracker.UITests.Tests;

public class AiUsageReportTests : IClassFixture<AppCollection>
{
  private readonly AppFixture _fixture;
  public AiUsageReportTests(AppCollection collection) => _fixture = collection.Fixture;

  [Fact]
  public async Task AiUsageReportPage_Loads_And_Shows_Header()
  {
    var page = await _fixture.NewPageAsync();
    var aiPage = new AiUsageReportPage(page);
    await aiPage.GotoAsync();
    Assert.Contains("AI Usage Report", await aiPage.GetHeaderAsync());
    Assert.True(await aiPage.HasAiUsageTableAsync());
  }
}
