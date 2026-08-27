using AatpTemplateTestSuite.Infra.Configuration;
using Microsoft.Playwright;
using Reqnroll;
using Reqnroll.BoDi;
using Serilog;

namespace AatpTemplateTestSuite.Hooks;

[Binding]
public sealed class AfterScenarioHooks
{
    private const string ArtifactsFolderName = "test-artifacts";

    private readonly IObjectContainer _scenarioContainer;
    private readonly TestSettings _settings;
    private readonly ScenarioContext _scenarioContext;

    public AfterScenarioHooks(
        IObjectContainer scenarioContainer,
        TestSettings settings,
        ScenarioContext scenarioContext)
    {
        _scenarioContainer = scenarioContainer;
        _settings = settings;
        _scenarioContext = scenarioContext;
    }

    [AfterScenario]
    public async Task CloseBrowserContextAsync()
    {
        var scenarioTitle = _scenarioContext.ScenarioInfo.Title;
        var hasFailed = _scenarioContext.TestError is not null;

        if (hasFailed)
        {
            Log.Error(_scenarioContext.TestError, "Scenario failed: {Scenario}", scenarioTitle);
        }
        else
        {
            Log.Information("Scenario passed: {Scenario}", scenarioTitle);
        }

        if (!_scenarioContainer.IsRegistered<IBrowserContext>())
        {
            return;
        }

        var browserContext = _scenarioContainer.Resolve<IBrowserContext>();

        if (hasFailed)
        {
            await CaptureScreenshotAsync(scenarioTitle);
        }

        if (_settings.TracingEnabled)
        {
            await StopTracingAsync(browserContext, scenarioTitle, hasFailed);
        }

        await browserContext.CloseAsync();
    }

    private async Task CaptureScreenshotAsync(string scenarioTitle)
    {
        if (!_scenarioContainer.IsRegistered<IPage>())
        {
            return;
        }

        var path = ArtifactPath($"{FileNameFor(scenarioTitle)}.png");
        await _scenarioContainer.Resolve<IPage>().ScreenshotAsync(new PageScreenshotOptions
        {
            Path = path,
            FullPage = true
        });

        Log.Warning("Failure screenshot written to {Path}", path);
    }

    private static async Task StopTracingAsync(IBrowserContext browserContext, string scenarioTitle, bool hasFailed)
    {
        if (!hasFailed)
        {
            await browserContext.Tracing.StopAsync();
            return;
        }

        var path = ArtifactPath($"{FileNameFor(scenarioTitle)}.zip");
        await browserContext.Tracing.StopAsync(new TracingStopOptions { Path = path });

        Log.Warning("Failure trace written to {Path}", path);
    }

    private static string ArtifactPath(string fileName)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, ArtifactsFolderName);
        Directory.CreateDirectory(directory);

        return Path.Combine(directory, fileName);
    }

    private static string FileNameFor(string scenarioTitle)
    {
        var safeTitle = string.Join("-", scenarioTitle.Split(Path.GetInvalidFileNameChars().Append(' ').ToArray(),
            StringSplitOptions.RemoveEmptyEntries));

        return $"{safeTitle}-{DateTime.Now:yyyyMMdd-HHmmss}";
    }
}
