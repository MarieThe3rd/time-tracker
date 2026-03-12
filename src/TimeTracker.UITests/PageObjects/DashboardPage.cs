using Microsoft.Playwright;
using TimeTracker.UITests.Infrastructure;

namespace TimeTracker.UITests.PageObjects;

public class DashboardPage(IPage page) : PageObjectBase(page)
{
    protected override string Route => "/";

    // Wave 5: the labels changed when date-range presets were added
    public ILocator TimeTodayCard => SummaryCard("Total Time");
    public ILocator AvgProductivityCard => SummaryCard("Avg Productivity");
    public ILocator EntriesTodayCard => SummaryCard("Total Entries");
    public ILocator AiTimeSavedCard => SummaryCard("AI Time Saved");
    public ILocator RecentEntriesCard => Page.Locator(".card-header", new() { HasText = "Recent Entries" });
    public ILocator RecentJournalCard => Page.Locator(".card-header", new() { HasText = "Recent Journal" });
    public ILocator TimeByCategoryCard => Page.Locator(".card-header", new() { HasText = "Time by Category" });

    public ILocator NavDashboard => Page.Locator("nav a", new() { HasText = "Dashboard" });
    public ILocator NavTimer => Page.Locator("nav a", new() { HasText = "Timer" });
    public ILocator NavJournal => Page.Locator("nav a", new() { HasText = "Journal" });
    public ILocator NavReports => Page.Locator("nav a", new() { HasText = "Reports" });
    public ILocator NavSettings => Page.Locator("nav a", new() { HasText = "Settings" });
    public ILocator NavAiUsage => Page.Locator("nav a", new() { HasText = "AI Usage" });

    // Wave 5: date range preset buttons
    public ILocator TodayPresetButton => Page.Locator("button.btn-sm", new() { HasText = "Today" });
    public ILocator ThisWeekPresetButton => Page.Locator("button.btn-sm", new() { HasText = "This Week" });
    public ILocator ThisMonthPresetButton => Page.Locator("button.btn-sm", new() { HasText = "This Month" });
    public ILocator CustomPresetButton => Page.Locator("button.btn-sm", new() { HasText = "Custom" });

    public ILocator DateLabel => Page.Locator(".text-muted.small.ms-2");

    // Custom date inputs — only visible when Custom preset is active
    public ILocator CustomFromInput => Page.Locator("input[type='date']").First;
    public ILocator CustomToInput => Page.Locator("input[type='date']").Last;

    private ILocator SummaryCard(string title) =>
        Page.GetByText(title, new() { Exact = true })
            .Locator("xpath=ancestor::div[contains(@class,'card-body')]");
}
