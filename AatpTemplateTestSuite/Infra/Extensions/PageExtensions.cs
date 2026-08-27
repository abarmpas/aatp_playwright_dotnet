using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Serilog;

namespace AatpTemplateTestSuite.Infra.Extensions;

public static class PageExtensions
{
    public static async Task NavigateToPathAsync(this IPage page, string path)
    {
        Log.Debug("Navigating to {Path}", path);

        await page.GotoAsync(path, new PageGotoOptions { WaitUntil = WaitUntilState.Load });

        // Client-rendered apps attach their event handlers after the scripts run. Interacting earlier
        // submits forms natively and loses the state the app expects to own.
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public static async Task WaitForPathAsync(this IPage page, string expectedPath, int? timeoutMs = null) =>
        await Assertions.Expect(page).ToHaveURLAsync(
            new Regex($"{Regex.Escape(expectedPath.TrimEnd('/'))}/?$"),
            new PageAssertionsToHaveURLOptions { Timeout = timeoutMs });

    public static async Task<bool> IsAtPathAsync(this IPage page, string expectedPath, int? timeoutMs = null)
    {
        try
        {
            await page.WaitForPathAsync(expectedPath, timeoutMs);
            return true;
        }
        catch (PlaywrightException)
        {
            Log.Debug("Page is at {ActualUrl} instead of {ExpectedPath}", page.Url, expectedPath);
            return false;
        }
    }
}
