using Microsoft.Playwright;
using TimeTracker.UITests.Infrastructure;

namespace TimeTracker.UITests.PageObjects;

public class ReportsPage(IPage page) : PageObjectBase(page)
{
    protected override string Route => "/reports";

    // Preset is a <select> with options: today, week, month, custom
    public ILocator PresetSelect => Page.Locator("select.form-select").First;

    // Date inputs — use stable IDs so adding future inputs doesn't break locators
    public ILocator FromDateInput => Page.Locator("#reportFrom");
    public ILocator ToDateInput => Page.Locator("#reportTo");
    public ILocator DayDateInput => Page.Locator("#reportDay");

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

    public async Task<IReadOnlyList<string>> GetAiChartDatasetLabelsAsync()
    {
        var labels = await Page.EvaluateAsync<string[]>(@"() => {
            const chart = window.Chart?.getChart('aiUsageChart');
            if (!chart) {
                return [];
            }

            return chart.data.datasets.map(d => d.label ?? '');
        }");

        return labels;
    }

    public async Task<IReadOnlyList<double>> GetAiChartDatasetDataAsync(int datasetIndex)
    {
        var data = await Page.EvaluateAsync<double[]>(@"(idx) => {
            const chart = window.Chart?.getChart('aiUsageChart');
            if (!chart || !chart.data?.datasets?.[idx]) {
                return [];
            }

            return chart.data.datasets[idx].data.map(value => Number(value ?? 0));
        }", datasetIndex);

        return data;
    }
}
