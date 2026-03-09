using System.Diagnostics;
using System.Net;
using Microsoft.Playwright;

namespace TimeTracker.UITests.Infrastructure;

/// <summary>
/// xUnit class fixture that starts the TimeTracker.Web process against the UITest
/// environment (separate LocalDB) and creates a shared Playwright browser.
/// One process + one browser is reused across all tests in a collection.
/// </summary>
public sealed class AppFixture : IAsyncLifetime
{
    public const string BaseUrl = "http://localhost:5299";

    private Process? _serverProcess;

    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Ensure Playwright browsers are installed (no-op if already present)
        Microsoft.Playwright.Program.Main(["install", "chromium"]);

        var exePath = Path.Combine(AppContext.BaseDirectory, "TimeTracker.Web.exe");
        if (!File.Exists(exePath))
            throw new FileNotFoundException(
                $"TimeTracker.Web.exe not found at {exePath}. Ensure the UITests project references the Web project.");

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = $"--urls {BaseUrl}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            // Set the working directory to the exe's location so ASP.NET Core uses that
            // as the ContentRoot, which is where appsettings.UITest.json is located.
            WorkingDirectory = AppContext.BaseDirectory,
        };
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        // Override the connection string so UI tests use an isolated database
        psi.Environment["ConnectionStrings__DefaultConnection"] =
            "Server=(localdb)\\mssqllocaldb;Database=TimeTrackerUITest;Trusted_Connection=True;MultipleActiveResultSets=true";

        _serverProcess = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start TimeTracker.Web process.");

        await WaitForServerAsync(BaseUrl, timeoutSeconds: 60);

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
            await Browser.DisposeAsync();

        Playwright?.Dispose();

        if (_serverProcess is not null && !_serverProcess.HasExited)
        {
            _serverProcess.Kill(entireProcessTree: true);
            await _serverProcess.WaitForExitAsync();
            _serverProcess.Dispose();
        }
    }

    /// <summary>Creates a fresh browser context + page for each test.</summary>
    public async Task<IPage> NewPageAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
        });
        var page = await context.NewPageAsync();
        // Wait for Blazor SignalR circuit to connect before tests interact
        page.SetDefaultTimeout(15_000);
        return page;
    }

    private static async Task WaitForServerAsync(string url, int timeoutSeconds = 60)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await http.GetAsync(url);
                if ((int)response.StatusCode < 500)
                    return;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { }

            await Task.Delay(300);
        }
        throw new TimeoutException($"Server at {url} did not become ready within {timeoutSeconds}s.");
    }
}
