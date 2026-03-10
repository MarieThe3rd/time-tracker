using Microsoft.Playwright;
using System.Threading.Tasks;

namespace TimeTracker.UITests.PageObjects;

public class AiUsageReportPage
{
  private readonly IPage _page;
  public AiUsageReportPage(IPage page) => _page = page;

  public async Task GotoAsync() => await _page.GotoAsync("/reports/ai-usage");
  public async Task<bool> HasAiUsageTableAsync() => await _page.Locator("table").IsVisibleAsync();
  public async Task<string> GetHeaderAsync() => await _page.Locator("h4").InnerTextAsync();
}
