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
    public ILocator AiSavingsSummaryCard => Page.Locator(".card-header", new() { HasText = "AI Savings Summary" });
    public ILocator AiSavingsText => Page.Locator("text=saved using AI");

    // Markdown preview — NOTE: Wave 4 replaced <pre> blocks on daily/weekly/review tabs
    // with structured HTML. This locator is kept for the AI tab download flow tests only.
    public ILocator MarkdownPreview => Page.Locator("pre").First;

    // Wave 4: HTML-rendered tab content locators
    public ILocator DailyNoteTable => Page.Locator(".card-body table.table").First;
    public ILocator WeeklyProgressBars => Page.Locator(".progress");
    public ILocator ReviewSuccessSection => Page.Locator(".badge.bg-success");
    public ILocator ReviewChallengeSection => Page.Locator(".badge.bg-warning");
    public ILocator ReviewLearningSection => Page.Locator(".badge.bg-info.text-dark");
    public ILocator DownloadMdButton => Page.Locator("button", new() { HasText = "Download .md" });
    public ILocator PushToObsidianButton => Page.Locator("button", new() { HasText = "Push to Obsidian" });

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
