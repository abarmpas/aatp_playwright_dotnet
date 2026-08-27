using AatpTemplateTestSuite.Infra.Configuration;
using Microsoft.Playwright;
using Serilog;

namespace AatpTemplateTestSuite.Infra.Drivers;

public sealed class PlaywrightDriver : IAsyncDisposable
{
    private readonly TestSettings _settings;

    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public PlaywrightDriver(TestSettings settings)
    {
        _settings = settings;
    }

    public async Task LaunchBrowserAsync()
    {
        EnsureBrowsersInstalled(_settings.Browser);

        _playwright = await Playwright.CreateAsync();
        _browser = await ResolveBrowserType(_playwright).LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = _settings.Headless
        });

        Log.Information(
            "Launched {Browser} {Version} in {Mode} mode",
            _settings.Browser,
            _browser.Version,
            _settings.Headless ? "headless" : "headed");
    }

    public async Task<IBrowserContext> CreateContextAsync()
    {
        if (_browser is null)
        {
            throw new InvalidOperationException(
                $"The browser has not been launched. {nameof(LaunchBrowserAsync)} must run before a context is created.");
        }

        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = _settings.BaseUrl,
            ViewportSize = new ViewportSize
            {
                Width = _settings.Viewport.Width,
                Height = _settings.Viewport.Height
            }
        });

        context.SetDefaultTimeout(_settings.DefaultTimeoutMs);
        context.SetDefaultNavigationTimeout(_settings.NavigationTimeoutMs);

        return context;
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        _playwright?.Dispose();
        _playwright = null;
    }

    private static void EnsureBrowsersInstalled(string browser)
    {
        var exitCode = Microsoft.Playwright.Program.Main(["install", browser.ToLowerInvariant()]);
        if (exitCode != 0)
        {
            throw new InvalidOperationException(
                $"Playwright could not install the '{browser}' browser (exit code {exitCode}). Run 'playwright install' manually and retry.");
        }

        Log.Debug("Playwright browser {Browser} is installed", browser);
    }

    private IBrowserType ResolveBrowserType(IPlaywright playwright) => _settings.Browser.ToLowerInvariant() switch
    {
        "chromium" => playwright.Chromium,
        "firefox" => playwright.Firefox,
        "webkit" => playwright.Webkit,
        _ => throw new ArgumentOutOfRangeException(
            nameof(_settings.Browser),
            _settings.Browser,
            "Supported browsers are Chromium, Firefox and WebKit.")
    };
}
