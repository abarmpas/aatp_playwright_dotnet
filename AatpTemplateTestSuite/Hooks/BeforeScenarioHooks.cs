using AatpTemplateTestSuite.Infra.Configuration;
using AatpTemplateTestSuite.Infra.Drivers;
using Microsoft.Playwright;
using Reqnroll;
using Reqnroll.BoDi;
using Serilog;

namespace AatpTemplateTestSuite.Hooks;

[Binding]
public sealed class BeforeScenarioHooks
{
    private readonly IObjectContainer _scenarioContainer;
    private readonly PlaywrightDriver _driver;
    private readonly TestSettings _settings;
    private readonly ScenarioContext _scenarioContext;

    public BeforeScenarioHooks(
        IObjectContainer scenarioContainer,
        PlaywrightDriver driver,
        TestSettings settings,
        ScenarioContext scenarioContext)
    {
        _scenarioContainer = scenarioContainer;
        _driver = driver;
        _settings = settings;
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario]
    public async Task OpenBrowserContextAsync()
    {
        Log.Information("Scenario starting: {Scenario}", _scenarioContext.ScenarioInfo.Title);

        var browserContext = await _driver.CreateContextAsync();

        if (_settings.TracingEnabled)
        {
            await browserContext.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = false
            });
        }

        var page = await browserContext.NewPageAsync();

        _scenarioContainer.RegisterInstanceAs(browserContext);
        _scenarioContainer.RegisterInstanceAs(page);
    }
}
