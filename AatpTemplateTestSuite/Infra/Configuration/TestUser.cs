namespace AatpTemplateTestSuite.Infra.Configuration;

public sealed class TestUser
{
    public const string SectionName = "TestUser";

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
