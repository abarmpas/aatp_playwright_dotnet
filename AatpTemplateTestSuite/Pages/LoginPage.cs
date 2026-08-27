using AatpTemplateTestSuite.Infra.Extensions;
using Microsoft.Playwright;
using Serilog;

namespace AatpTemplateTestSuite.Pages;

public sealed class LoginPage : BasePage
{
    private const string LoginPath = "/";

    public LoginPage(IPage page)
        : base(page)
    {
    }

    private const string LoginFormSelector = "[data-test-id=\"login-form\"]";
    private const string EmailInputSelector = $"{LoginFormSelector} [data-test-id=\"email-input\"]";
    private const string PasswordInputSelector = $"{LoginFormSelector} [data-test-id=\"password-input\"]";
    private const string SignInButtonSelector = $"{LoginFormSelector} [data-test-id=\"login-button\"]";

    private ILocator LoginFormLocator => Page.Locator(LoginFormSelector);

    private ILocator EmailInputLocator => Page.Locator(EmailInputSelector);

    private ILocator PasswordInputLocator => Page.Locator(PasswordInputSelector);

    private ILocator SignInButtonLocator => Page.Locator(SignInButtonSelector);

    public async Task NavigateAsync()
    {
        await NavigateToAsync(LoginPath);
        await LoginFormLocator.WaitForLocatorToBeVisibleAsync();
    }

    public async Task EnterEmailAsync(string email) => await EmailInputLocator.FillAsync(email);

    public async Task EnterPasswordAsync(string password) => await PasswordInputLocator.FillAsync(password);

    public async Task ClickSignInAsync() => await SignInButtonLocator.ClickAsync();

    public async Task SignInAsync(string email, string password)
    {
        Log.Information("Signing in as {Email}", email);

        await EnterEmailAsync(email);
        await EnterPasswordAsync(password);
        await ClickSignInAsync();
    }

    public async Task VerifySignInFormIsDisplayedAsync()
    {
        var isEmailInputVisible = await EmailInputLocator.IsLocatorVisibleAsync();
        var isPasswordInputVisible = await PasswordInputLocator.IsLocatorVisibleAsync();
        var isSignInButtonVisible = await SignInButtonLocator.IsLocatorVisibleAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                isEmailInputVisible,
                Is.True,
                "The email input is not displayed on the sign in page.");
            Assert.That(
                isPasswordInputVisible,
                Is.True,
                "The password input is not displayed on the sign in page.");
            Assert.That(
                isSignInButtonVisible,
                Is.True,
                "The sign in button is not displayed on the sign in page.");
        }
    }
}
