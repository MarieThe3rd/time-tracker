using Microsoft.Playwright;
using TimeTracker.UITests.Infrastructure;

namespace TimeTracker.UITests.PageObjects;

public class ReportsPage(IPage page) : PageObjectBase(page)
{
    protected override string Route => "/reports";

    // Preset is a <select> with options: today, week, month, custom
    public ILocator PresetSelect => Page.Locator("select.form-select").First;

    // Date inputs — scoped to the date-range card (first card on page) to avoid
    // matching the QuickAdd panel's hidden date input
    private ILocator DateRangeCard => Page.Locator(".card").First;
    public ILocator FromDateInput => DateRangeCard.Locator("input[type='date']").First;
    public ILocator ToDateInput => DateRangeCard.Locator("input[type='date']").Last;

    // Tab nav — use :text-is() for exact match; "Summary" alone would also match "Weekly Summary"
    public ILocator SummaryTab => Page.Locator("button.nav-link:text-is('Summary')");
    public ILocator AiInsightsTab => Page.Locator("button.nav-link", new() { HasText = "AI Insights" });
    public ILocator DailyNoteTab => Page.Locator("button.nav-link", new() { HasText = "Daily Note" });
    public ILocator WeeklySummaryTab => Page.Locator("button.nav-link", new() { HasText = "Weekly Summary" });
    public ILocator ReviewExportTab => Page.Locator("button.nav-link", new() { HasText = "Review Export" });

    public ILocator AiEmptyState => Page.Locator("text=No AI-assisted entries in this range.");
    public ILocator AiUsageChart => Page.Locator("#aiUsageChart");
    public ILocator AiWeeklySummaryCard => Page.Locator(".card-header", new() { HasText = "Weekly AI Summary" });

    // Markdown preview
    public ILocator MarkdownPreview => Page.Locator("pre").First;
}
