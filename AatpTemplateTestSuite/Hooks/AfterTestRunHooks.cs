using AatpTemplateTestSuite.Infra.Drivers;
using Reqnroll;
using Serilog;

namespace AatpTemplateTestSuite.Hooks;

[Binding]
public sealed class AfterTestRunHooks
{
    [AfterTestRun]
    public static async Task ShutdownTestRunAsync(PlaywrightDriver driver)
    {
        await driver.DisposeAsync();

        Log.Information("Test run finished");
        await Log.CloseAndFlushAsync();
    }
}
