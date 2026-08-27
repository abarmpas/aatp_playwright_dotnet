using AatpTemplateTestSuite.Infra;
using AatpTemplateTestSuite.Infra.Drivers;
using Microsoft.Extensions.Configuration;
using Reqnroll;
using Reqnroll.BoDi;
using Serilog;
using Serilog.Events;

namespace AatpTemplateTestSuite.Hooks;

[Binding]
public sealed class BeforeTestRunHooks
{
    private const string MinimumLevelKey = "Serilog:MinimumLevel";

    [BeforeTestRun(Order = 0)]
    public static void ConfigureTestRun(IObjectContainer testRunContainer)
    {
        var configuration = testRunContainer.RegisterTestRunDependencies();
        Log.Logger = CreateLogger(configuration);

        Log.Information("Test run starting");
    }

    [BeforeTestRun(Order = 1)]
    public static async Task LaunchBrowserAsync(PlaywrightDriver driver) => await driver.LaunchBrowserAsync();

    private static Serilog.ILogger CreateLogger(IConfiguration configuration) => new LoggerConfiguration()
        .MinimumLevel.Is(ResolveMinimumLevel(configuration))
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    private static LogEventLevel ResolveMinimumLevel(IConfiguration configuration)
    {
        var configuredLevel = configuration[MinimumLevelKey];

        return Enum.TryParse(configuredLevel, ignoreCase: true, out LogEventLevel level)
            ? level
            : LogEventLevel.Information;
    }
}
