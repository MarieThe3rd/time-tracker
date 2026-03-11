using Microsoft.Playwright;
using TimeTracker.UITests.Infrastructure;

namespace TimeTracker.UITests.PageObjects;

public class DashboardPage(IPage page) : PageObjectBase(page)
{
    protected override string Route => "/";

    public ILocator TimeTodayCard => SummaryCard("Time Today");
    public ILocator AvgProductivityCard => SummaryCard("Avg Productivity");
    public ILocator EntriesTodayCard => SummaryCard("Entries Today");
    public ILocator AiTimeSavedCard => SummaryCard("AI Time Saved This Week");
    public ILocator RecentEntriesCard => Page.Locator(".card-header", new() { HasText = "Recent Entries" });
    public ILocator RecentJournalCard => Page.Locator(".card-header", new() { HasText = "Recent Journal" });
    public ILocator TimeByCategoryCard => Page.Locator(".card-header", new() { HasText = "Time by Category" });

    public ILocator NavDashboard => Page.Locator("nav a", new() { HasText = "Dashboard" });
    public ILocator NavTimer => Page.Locator("nav a", new() { HasText = "Timer" });
    public ILocator NavJournal => Page.Locator("nav a", new() { HasText = "Journal" });
    public ILocator NavReports => Page.Locator("nav a", new() { HasText = "Reports" });
    public ILocator NavSettings => Page.Locator("nav a", new() { HasText = "Settings" });
    public ILocator NavAiUsage => Page.Locator("nav a", new() { HasText = "AI Usage" });

    private ILocator SummaryCard(string title) =>
        Page.GetByText(title, new() { Exact = true })
            .Locator("xpath=ancestor::div[contains(@class,'card-body')]");
}
