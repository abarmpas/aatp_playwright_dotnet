using AatpTemplateTestSuite.Infra.Extensions;
using Microsoft.Playwright;

namespace AatpTemplateTestSuite.Pages;

public abstract class BasePage
{
    protected BasePage(IPage page)
    {
        Page = page;
    }

    protected IPage Page { get; }

    protected async Task NavigateToAsync(string path) => await Page.NavigateToPathAsync(path);
}
