using AatpTemplateTestSuite.Pages;
using Reqnroll;

namespace AatpTemplateTestSuite.Steps;

[Binding]
public sealed class DashboardSteps
{
    private readonly DashboardPage _dashboardPage;

    public DashboardSteps(DashboardPage dashboardPage)
    {
        _dashboardPage = dashboardPage;
    }

    [Then("the dashboard is displayed")]
    public async Task ThenTheDashboardIsDisplayedAsync() => await _dashboardPage.VerifyDashboardIsDisplayedAsync();
}
