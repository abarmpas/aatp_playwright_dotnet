using System.Reflection;
using AatpTemplateTestSuite.Infra.Configuration;
using Microsoft.Extensions.Configuration;
using Reqnroll.BoDi;

namespace AatpTemplateTestSuite.Infra;

public static class DependencyInjection
{
    public static IConfiguration RegisterTestRunDependencies(this IObjectContainer container)
    {
        var configuration = BuildConfiguration();

        container.RegisterInstanceAs(configuration);
        container.RegisterInstanceAs(Bind<TestSettings>(configuration, TestSettings.SectionName));
        container.RegisterInstanceAs(Bind<TestUser>(configuration, TestUser.SectionName));
        container.RegisterInstanceAs(Bind<InvalidTestUser>(configuration, InvalidTestUser.SectionName));

        return configuration;
    }

    private static IConfiguration BuildConfiguration() => new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"appsettings.{EnvironmentName()}.json", optional: true, reloadOnChange: false)
        .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
        .AddEnvironmentVariables()
        .Build();

    private static string EnvironmentName() =>
        Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

    private static T Bind<T>(IConfiguration configuration, string sectionName)
        where T : class, new()
    {
        var section = configuration.GetSection(sectionName);
        if (!section.Exists())
        {
            throw new InvalidOperationException(
                $"Configuration section '{sectionName}' is missing. Check appsettings.json in the output directory.");
        }

        return section.Get<T>() ?? new T();
    }
}
