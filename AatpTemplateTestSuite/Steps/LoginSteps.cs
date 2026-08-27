using AatpTemplateTestSuite.Infra.Configuration;
using AatpTemplateTestSuite.Pages;
using Reqnroll;

namespace AatpTemplateTestSuite.Steps;

[Binding]
public sealed class LoginSteps
{
    private readonly LoginPage _loginPage;
    private readonly TestUser _testUser;

    public LoginSteps(LoginPage loginPage, TestUser testUser)
    {
        _loginPage = loginPage;
        _testUser = testUser;
    }

    [Given("the user is on the sign in page")]
    public async Task GivenTheUserIsOnTheSignInPageAsync()
    {
        await _loginPage.NavigateAsync();
        await _loginPage.VerifySignInFormIsDisplayedAsync();
    }

    [When("the user signs in with valid credentials")]
    public async Task WhenTheUserSignsInWithValidCredentialsAsync()
    {
        Assert.That(
            _testUser.Email,
            Is.Not.Empty,
            "No test user email is configured. Set TestUser:Email in appsettings.json, user-secrets or the environment.");
        Assert.That(
            _testUser.Password,
            Is.Not.Empty,
            "No test user password is configured. Set TestUser:Password in user-secrets or the environment.");

        await _loginPage.SignInAsync(_testUser.Email, _testUser.Password);
    }
}
