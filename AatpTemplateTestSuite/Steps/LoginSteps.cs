using AatpTemplateTestSuite.Infra.Configuration;
using AatpTemplateTestSuite.Pages;
using Reqnroll;

namespace AatpTemplateTestSuite.Steps;

[Binding]
public sealed class LoginSteps
{
    private readonly LoginPage _loginPage;
    private readonly TestUser _testUser;
    private readonly InvalidTestUser _invalidTestUser;

    public LoginSteps(LoginPage loginPage, TestUser testUser, InvalidTestUser invalidTestUser)
    {
        _loginPage = loginPage;
        _testUser = testUser;
        _invalidTestUser = invalidTestUser;
    }

    [Given("the user is on the sign in page")]
    public async Task GivenTheUserIsOnTheSignInPageAsync() => await _loginPage.NavigateAsync();

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

    [When("the user signs in with invalid credentials")]
    public async Task WhenTheUserSignsInWithInvalidCredentialsAsync()
    {
        Assert.That(
            _invalidTestUser.Email,
            Is.Not.Empty,
            "No invalid test user email is configured. Set InvalidTestUser:Email in appsettings.json, user-secrets or the environment.");
        Assert.That(
            _invalidTestUser.Password,
            Is.Not.Empty,
            "No invalid test user password is configured. Set InvalidTestUser:Password in appsettings.json, user-secrets or the environment.");
        Assert.That(
            _invalidTestUser.Email != _testUser.Email || _invalidTestUser.Password != _testUser.Password,
            Is.True,
            "InvalidTestUser credentials must differ from the configured TestUser credentials.");

        await _loginPage.SignInAsync(_invalidTestUser.Email, _invalidTestUser.Password);
    }

    [Then("sign in failure is shown")]
    public async Task ThenSignInFailureIsShownAsync() => await _loginPage.VerifySignInErrorIsDisplayedAsync();

    [Then("the user remains on the sign in page")]
    public async Task ThenTheUserRemainsOnTheSignInPageAsync() =>
        await _loginPage.VerifySignInFormIsDisplayedAsync();
}
