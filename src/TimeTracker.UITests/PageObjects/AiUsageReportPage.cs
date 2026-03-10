using Microsoft.Playwright;
using TimeTracker.UITests.Infrastructure;

namespace TimeTracker.UITests.PageObjects;

public class AiUsageReportPage(IPage page) : PageObjectBase(page)
{
  protected override string Route => "/reports/ai-usage";

  private ILocator DateRangeCard => Page.Locator(".card").First;
  public ILocator FromDateInput => DateRangeCard.Locator("input[type='date']").First;
  public ILocator ToDateInput => DateRangeCard.Locator("input[type='date']").Last;
  public ILocator EmptyState => Page.Locator("text=No AI usage entries in this range.");
  public ILocator DetailsTable => Page.Locator("table").First;
  public ILocator ChartCanvas => Page.Locator("#aiUsageChart");
  public ILocator SidebarLink => Page.Locator("a[href='/reports/ai-usage']");
}
