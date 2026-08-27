namespace AatpTemplateTestSuite.Infra.Configuration;

public sealed class InvalidTestUser
{
    public const string SectionName = "InvalidTestUser";

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
