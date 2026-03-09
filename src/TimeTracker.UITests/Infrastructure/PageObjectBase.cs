using Microsoft.Playwright;

namespace TimeTracker.UITests.Infrastructure;

/// <summary>
/// Base class for all Page Object Models.
/// Provides navigation helpers and common wait utilities.
/// </summary>
public abstract class PageObjectBase(IPage page)
{
    protected readonly IPage Page = page;

    protected string BaseUrl => AppFixture.BaseUrl;

    /// <summary>Navigate to the page's route and wait for Blazor to connect.</summary>
    public async Task GotoAsync()
    {
        await Page.GotoAsync(Route);
        // Wait for the h4 heading (present in SSR static HTML immediately)
        await Page.Locator("h4").First.WaitForAsync(new() { Timeout = 15_000 });
        // Wait for data-blazor-ready="true" — set by MainLayout.OnAfterRenderAsync once
        // the SignalR circuit connects and Blazor's interactive rendering is live.
        // Falls back to a 3-second delay if the attribute doesn't appear within 8 seconds
        // (e.g. if JS interop is unavailable in the test environment).
        try
        {
            await Page.WaitForSelectorAsync("[data-blazor-ready='true']",
                new() { Timeout = 8_000, State = WaitForSelectorState.Attached });
        }
        catch (TimeoutException)
        {
            // Fallback: give Blazor a flat extra wait
            await Task.Delay(3_000);
        }
    }

    protected abstract string Route { get; }

    /// <summary>
    /// Waits briefly after a Blazor Server interaction (button click, select change, etc.)
    /// for the circuit to process the event and re-render the DOM.
    /// </summary>
    public async Task WaitForBlazorAsync() =>
        await Task.Delay(1_500);

    /// <summary>Returns text content of the page's main h4 heading.</summary>
    public async Task<string> GetHeadingAsync() =>
        await Page.Locator("h4").First.InnerTextAsync();
}
