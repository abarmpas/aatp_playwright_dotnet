namespace AatpTemplateTestSuite.Infra.Configuration;

public sealed class TestSettings
{
    public const string SectionName = "TestSettings";

    public string BaseUrl { get; init; } = string.Empty;

    public string Browser { get; init; } = "Chromium";

    public bool Headless { get; init; } = true;

    public int DefaultTimeoutMs { get; init; } = 15000;

    public int NavigationTimeoutMs { get; init; } = 30000;

    public bool TracingEnabled { get; init; } = true;

    public ViewportSettings Viewport { get; init; } = new();
}
