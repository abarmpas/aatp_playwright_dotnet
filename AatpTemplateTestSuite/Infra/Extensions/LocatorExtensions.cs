using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Serilog;

namespace AatpTemplateTestSuite.Infra.Extensions;

public static class LocatorExtensions
{
    private static readonly Regex AnyValue = new(".+", RegexOptions.Compiled);

    public static async Task<bool> IsLocatorVisibleAsync(this ILocator locator, int? timeoutMs = null)
    {
        try
        {
            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });

            return true;
        }
        catch (PlaywrightException)
        {
            Log.Debug("Locator {Locator} did not become visible", locator);
            return false;
        }
    }

    public static async Task<bool> IsLocatorHiddenAsync(this ILocator locator, int? timeoutMs = null)
    {
        try
        {
            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = timeoutMs
            });

            return true;
        }
        catch (PlaywrightException)
        {
            Log.Debug("Locator {Locator} did not become hidden", locator);
            return false;
        }
    }

    public static async Task WaitForLocatorToBeVisibleAsync(this ILocator locator, int? timeoutMs = null) =>
        await Assertions.Expect(locator).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
        {
            Timeout = timeoutMs
        });

    public static async Task<string> WaitForLocatorInnerTextAsync(this ILocator locator, int? timeoutMs = null)
    {
        await locator.WaitForLocatorToBeVisibleAsync(timeoutMs);
        return await locator.InnerTextAsync();
    }

    public static async Task<string> WaitForAttributeToBePopulatedAsync(
        this ILocator locator,
        string attributeName,
        int? timeoutMs = null)
    {
        await Assertions.Expect(locator).ToHaveAttributeAsync(
            attributeName,
            AnyValue,
            new LocatorAssertionsToHaveAttributeOptions { Timeout = timeoutMs });

        return await locator.GetAttributeAsync(attributeName) ?? string.Empty;
    }

    public static async Task<int> CountElementsAsync(this ILocator locator, int? timeoutMs = null)
    {
        if (!await locator.IsLocatorAttachedAsync(timeoutMs))
        {
            return 0;
        }

        return await locator.CountAsync();
    }

    public static async Task<IReadOnlyList<ILocator>> AllElementsAsync(this ILocator locator, int? timeoutMs = null)
    {
        if (!await locator.IsLocatorAttachedAsync(timeoutMs))
        {
            return [];
        }

        return await locator.AllAsync();
    }

    private static async Task<bool> IsLocatorAttachedAsync(this ILocator locator, int? timeoutMs)
    {
        try
        {
            await locator.First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Attached,
                Timeout = timeoutMs
            });

            return true;
        }
        catch (PlaywrightException)
        {
            Log.Debug("No element matched locator {Locator}", locator);
            return false;
        }
    }
}
