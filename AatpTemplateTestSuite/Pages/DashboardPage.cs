using AatpTemplateTestSuite.Infra.Extensions;
using Microsoft.Playwright;

namespace AatpTemplateTestSuite.Pages;

public sealed class DashboardPage : BasePage
{
    private const string DashboardPath = "/dashboard";

    public DashboardPage(IPage page)
        : base(page)
    {
    }

    private const string DashboardContainerSelector = "[data-test-id=\"dashboard-container\"]";
    private const string DashboardTitleSelector = $"{DashboardContainerSelector} [data-test-id=\"dashboard-title\"]";
    private const string ProductsGridSelector = $"{DashboardContainerSelector} [data-test-id=\"products-grid\"]";
    private const string ProductCardSelector = $"{ProductsGridSelector} [data-test-id^=\"product-card-\"]";

    private ILocator DashboardContainerLocator => Page.Locator(DashboardContainerSelector);

    private ILocator DashboardTitleLocator => Page.Locator(DashboardTitleSelector);

    private ILocator ProductsGridLocator => Page.Locator(ProductsGridSelector);

    private ILocator ProductCardLocator => Page.Locator(ProductCardSelector);

    public async Task<string> GetTitleAsync() => await DashboardTitleLocator.WaitForLocatorInnerTextAsync();

    public async Task<int> GetProductCountAsync() => await ProductCardLocator.CountElementsAsync();

    public async Task VerifyDashboardIsDisplayedAsync()
    {
        await Page.WaitForPathAsync(DashboardPath);

        var isContainerVisible = await DashboardContainerLocator.IsLocatorVisibleAsync();
        var isProductsGridVisible = await ProductsGridLocator.IsLocatorVisibleAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                isContainerVisible,
                Is.True,
                "The dashboard container is not displayed after signing in.");
            Assert.That(
                isProductsGridVisible,
                Is.True,
                "The products grid is not displayed on the dashboard.");
        }
    }

    public async Task VerifyDashboardIsNotDisplayedAsync()
    {
        var isDashboardHidden = await DashboardContainerLocator.IsLocatorHiddenAsync();

        Assert.That(
            isDashboardHidden,
            Is.True,
            "The dashboard is displayed after an unsuccessful sign in attempt.");
    }
}
