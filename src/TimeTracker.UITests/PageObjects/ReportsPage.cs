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
    public ILocator DailyNoteTab => Page.Locator("button.nav-link", new() { HasText = "Daily Note" });
    public ILocator WeeklySummaryTab => Page.Locator("button.nav-link", new() { HasText = "Weekly Summary" });
    public ILocator ReviewExportTab => Page.Locator("button.nav-link", new() { HasText = "Review Export" });

    // Markdown preview
    public ILocator MarkdownPreview => Page.Locator("pre").First;
}
