using Microsoft.Playwright;
using TimeTracker.UITests.Infrastructure;

namespace TimeTracker.UITests.PageObjects;

public class DashboardPage(IPage page) : PageObjectBase(page)
{
    protected override string Route => "/";

    // The three summary stat cards share .col-sm-4; use Nth (0-indexed) to avoid ambiguity.
    public ILocator TimeTodayCard => Page.Locator(".col-sm-4").Nth(0).Locator(".card-body");
    public ILocator AvgProductivityCard => Page.Locator(".col-sm-4").Nth(1).Locator(".card-body");
    public ILocator EntriesTodayCard => Page.Locator(".col-sm-4").Nth(2).Locator(".card-body");
    public ILocator RecentEntriesCard => Page.Locator(".card-header", new() { HasText = "Recent Entries" });
    public ILocator RecentJournalCard => Page.Locator(".card-header", new() { HasText = "Recent Journal" });
    public ILocator TimeByCategoryCard => Page.Locator(".card-header", new() { HasText = "Time by Category" });

    public ILocator NavDashboard => Page.Locator("nav a", new() { HasText = "Dashboard" });
    public ILocator NavTimer => Page.Locator("nav a", new() { HasText = "Timer" });
    public ILocator NavJournal => Page.Locator("nav a", new() { HasText = "Journal" });
    public ILocator NavReports => Page.Locator("nav a", new() { HasText = "Reports" });
    public ILocator NavSettings => Page.Locator("nav a", new() { HasText = "Settings" });
}
