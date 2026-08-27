---
applyTo: '**'
---

# Automation Instructions

Rule: Page Object Model (POM) pages should contain locators, reusable UI actions, and reusable UI verification helpers. They should NOT contain business logic or scenario orchestration.
Rule: Steps should contain business logic, scenario orchestration, and scenario-level assertions, not low-level page object logic.
Rule: Prefer utilizing extension methods from the Extensions folder, such as LocatorExtensions and PageExtensions.
Rule: Organize methods by declaring public methods first and private methods last.
Rule: Avoid using force: true on clicks, as it leads to flaky interactions.
Rule: Prefer data-test-id or data-qa selectors for element targeting; use chained CSS with scope and unique identifiers (e.g., [data-scope="login-form"] [data-test-id="email-input"] or [data-qa="submit-button"]).
Rule: Avoid absolute XPaths and text-only selectors for element targeting.
Rule: Suffix ILocator properties with Locator (e.g., protected ILocator GameThumbLocator => ...).
Rule: Suffix string selector constants with Selector (e.g., private const string SubmitButtonSelector = ...).
Rule: Suffix async methods with Async (e.g., public async Task CloseMobileDrawerAsync()).
Rule: Do not suffix non-async methods with Async (e.g., private static void InitializeJiraClient()).
Rule: Rely on Playwright’s auto-wait mechanism for element interactions; avoid hard delays (Task.Delay) and thread sleeps.
Rule: For explicit waits, prefer project extension methods first. If no project helper fits, use Expect(locator).ToBeVisibleAsync() or WaitForSelectorAsync.
Rule: Keep scenario-level assertions in Step Definitions. Reusable UI verification methods may live in Page Objects when they group related UI checks with Assert.Multiple / Assert.MultipleAsync.
Rule: Always use Assert.That(...) constraint model syntax for assertions (never Assert.AreEqual, Assert.IsTrue, etc.).
Rule: Always use clear failure messages in assertions.
Rule: Use soft assertions only when multiple failures in a single scenario need to be collected.

## Locator Access & Naming

Rule: All locators and selectors in Page Objects must be private. Interactions should be exposed through public methods.
Rule: Prefix Page Object verification methods with Verify (e.g., VerifyErrorMessageIsDisplayedAsync()). Action methods start with Click, Enter, Select, Navigate, etc.

## Page Object Class Ordering

Rule: Follow this member ordering inside Page Object classes:

1. Fields (constants, readonly fields)
2. Constructor
3. Selectors and Locators (private)
4. Public methods (actions and verification helpers)
5. Private methods (helpers)

## Step Definitions

Rule: Do not use `using Microsoft.Playwright;` or `using FluentAssertions;` in Step Definition files. Steps must call Page Object methods or shared helpers rather than implementing direct locator-based UI logic.
Rule: Merge multiple common repetitive actions into a single reusable business-level step. For example, instead of separate steps for fill-stake / click-place-bet / verify-receipt, create one step like "user places a single bet" that encapsulates the full behavior.
Rule: When a step uses methods from multiple pages, place it in a Steps file named after the business segment (e.g., BettingSteps.cs). When a step uses only one page, place it in the corresponding PageNameSteps.cs.

## Extension Methods Over Raw Playwright

Rule: Use project extension methods instead of raw Playwright equivalents for built-in retry and wait logic whenever an equivalent helper exists:

- Use IsLocatorVisibleAsync() / IsLocatorHiddenAsync() instead of IsVisibleAsync().
- Use WaitForLocatorToBeVisibleAsync() instead of ad-hoc locator visibility waits.
- Use WaitForLocatorInnerTextAsync() instead of InnerTextAsync().
- Use WaitForAttributeToBePopulatedAsync() instead of GetAttributeAsync().
- Use CountElementsAsync() instead of CountAsync().
- Use AllElementsAsync() instead of AllAsync().

## Playwright How-To Patterns

## iFrames

```csharp
// By name attribute
var frame = _page.Frame("frame-login");
// By URL pattern
var frame = _page.FrameByUrl("*domain.");
// Interact within the frame
await frame.Locator(frameSelector).ClickAsync();
```

## New Tabs

```csharp
var newTab = await _browser.Contexts[0].RunAndWaitForPageAsync(async () =>
{
    await _page.Locator(mySelector).ClickAsync();
});
await newTab.Locator(tabSelector).IsLocatorVisibleAsync();
```

## Popups

```csharp
var popUp = await _page.RunAndWaitForPopupAsync(async () =>
{
    await _page.Locator(mySelector).ClickAsync();
});
await popUp.Locator(popupSelector).IsLocatorVisibleAsync();
```

## Wait for Response

```csharp
var response = await _page.RunAndWaitForResponseAsync(async () =>
{
    await _page.Locator(mySelector).ClickAsync();
}, res => res.Url.Contains(partOfTheURL) && res.Status == 200);
```

## Adding Cookies

```csharp
var customCookies = new Dictionary<string, string>
{
    { CookieName, "value" }
};
await _browserContext.AddCookiesAsync(_scenarioContext, customCookies);
```
