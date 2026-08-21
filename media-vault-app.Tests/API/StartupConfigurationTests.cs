using media_vault_app.API.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace media_vault_app.Tests.API;

public sealed class StartupConfigurationTests
{
    [Theory]
    [InlineData("Jwt:SecretKey", "short", "secret key")]
    [InlineData("Jwt:Issuer", " ", "issuer")]
    [InlineData("Jwt:Audience", "", "audience")]
    [InlineData("Jwt:ExpiryMinutes", "0", "expiry")]
    [InlineData("Jwt:ExpiryMinutes", "10081", "expiry")]
    public void InvalidJwtConfiguration_FailsDuringHostStartup(
        string key,
        string value,
        string expectedMessage)
    {
        using var factory = new StartupConfigurationFactory(
            "Staging",
            new Dictionary<string, string?> { [key] = value });

        var exception = AssertStartupFailure(factory);

        Assert.Contains(expectedMessage, exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("ExternalApis:Rawg:BaseUrl", "")]
    [InlineData("ExternalApis:Rawg:BaseUrl", "rawg.test")]
    [InlineData("ExternalApis:Rawg:BaseUrl", "http://rawg.test/")]
    [InlineData("ExternalApis:Tmdb:BaseUrl", "")]
    [InlineData("ExternalApis:Tmdb:BaseUrl", "tmdb.test")]
    [InlineData("ExternalApis:Tmdb:BaseUrl", "http://tmdb.test/")]
    [InlineData("ExternalApis:GoogleBooks:BaseUrl", "")]
    [InlineData("ExternalApis:GoogleBooks:BaseUrl", "books.test")]
    [InlineData("ExternalApis:GoogleBooks:BaseUrl", "http://books.test/")]
    public void InvalidProviderBaseUrl_FailsDuringHostStartup(string key, string value)
    {
        using var factory = new StartupConfigurationFactory(
            "Staging",
            new Dictionary<string, string?> { [key] = value });

        var exception = AssertStartupFailure(factory);

        Assert.Contains("BaseUrl", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-origin")]
    [InlineData("ftp://localhost:3000")]
    [InlineData("https://*.example.test")]
    [InlineData("https://example.test/")]
    [InlineData("https://example.test/path")]
    public void InvalidCorsOrigin_FailsDuringHostStartup(string origin)
    {
        using var factory = new StartupConfigurationFactory(
            "Staging",
            new Dictionary<string, string?> { ["Cors:AllowedOrigins:0"] = origin });

        var exception = AssertStartupFailure(factory);

        Assert.Contains("Cors:AllowedOrigins", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MissingCorsOrigin_FailsDuringProductionHostStartup()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"mediavault-cors-startup-{Guid.NewGuid():N}.db");

        try
        {
            using var factory = new StartupConfigurationFactory(
                "Production",
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = $"Data Source={databasePath}"
                },
                includeCorsOrigin: false);

            var exception = AssertStartupFailure(factory);

            Assert.Contains("Cors:AllowedOrigins", exception.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(databasePath))
                File.Delete(databasePath);
        }
    }

    [Fact]
    public void BlankProductionConnectionString_FailsDuringHostStartup()
    {
        using var factory = new StartupConfigurationFactory(
            "Production",
            new Dictionary<string, string?> { ["ConnectionStrings:Default"] = " " });

        var exception = AssertStartupFailure(factory);

        Assert.Contains(
            "Connection string 'Default'",
            exception.ToString(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Data Source=mediavault.db", "absolute")]
    [InlineData("Data Source=:memory:", "persistent")]
    public void UnsafeProductionSqliteConnectionString_FailsDuringHostStartup(
        string connectionString,
        string expectedMessage)
    {
        using var factory = new StartupConfigurationFactory(
            "Production",
            new Dictionary<string, string?> { ["ConnectionStrings:Default"] = connectionString });

        var exception = AssertStartupFailure(factory);

        Assert.Contains(expectedMessage, exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AbsoluteProductionSqliteConnectionString_StartsHost()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"mediavault-startup-{Guid.NewGuid():N}.db");

        try
        {
            using (var factory = new StartupConfigurationFactory(
                "Production",
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = $"Data Source={databasePath}"
                }))
            {
                _ = factory.Server;
            }
        }
        finally
        {
            if (File.Exists(databasePath))
                File.Delete(databasePath);
        }
    }

    [Fact]
    public void ValidConfiguration_StartsHostWithTestOverrides()
    {
        using var factory = new StartupConfigurationFactory("Staging");

        _ = factory.Server;

        var jwtOptions = factory.Services
            .GetRequiredService<IOptions<JwtOptions>>()
            .Value;

        Assert.Equal(JwtOptions.DefaultExpiryMinutes, jwtOptions.ExpiryMinutes);
    }

    private static Exception AssertStartupFailure(StartupConfigurationFactory factory) =>
        Assert.ThrowsAny<Exception>(() => _ = factory.Server);

    private sealed class StartupConfigurationFactory(
        string environment,
        IReadOnlyDictionary<string, string?>? overrides = null,
        bool includeCorsOrigin = true)
        : WebApplicationFactory<media_vault_app.API.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(environment);
            builder.UseSetting("ConnectionStrings:Default", "Data Source=:memory:");
            builder.UseSetting("ExternalApis:Rawg:BaseUrl", "https://rawg.test/");
            builder.UseSetting("ExternalApis:Rawg:ApiKey", "test-key");
            builder.UseSetting("ExternalApis:Tmdb:BaseUrl", "https://tmdb.test/");
            builder.UseSetting("ExternalApis:Tmdb:ApiAccessToken", "test-token");
            builder.UseSetting("ExternalApis:GoogleBooks:BaseUrl", "https://books.test/");
            builder.UseSetting("ExternalApis:GoogleBooks:ApiKey", "test-key");
            builder.UseSetting("Jwt:SecretKey", "integration-test-signing-key-at-least-32-bytes");
            builder.UseSetting("Jwt:Issuer", "MediaVault.Tests");
            builder.UseSetting("Jwt:Audience", "MediaVault.Tests");
            if (includeCorsOrigin)
                builder.UseSetting("Cors:AllowedOrigins:0", "https://localhost:61366");

            if (overrides is null)
                return;

            foreach (var (key, value) in overrides)
                builder.UseSetting(key, value);
        }
    }
}
