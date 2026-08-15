using System.Net;
using System.Text.Json;
using media_vault_app.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace media_vault_app.Tests.API;

public sealed class DatabaseReadinessTests
{
    [Fact]
    public async Task CurrentDatabase_ReturnsHealthyReadinessReport()
    {
        var databasePath = CreateDatabasePath();
        var factory = new DatabaseReadinessFactory(databasePath);

        try
        {
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.MigrateAsync();
            }

            using var client = factory.CreateClient();
            using var response = await client.GetAsync("/health/ready");
            using var document = await ParseResponseAsync(response);
            var database = document.RootElement
                .GetProperty("checks")
                .GetProperty("database");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Healthy", document.RootElement.GetProperty("status").GetString());
            Assert.Equal("Healthy", database.GetProperty("status").GetString());
            Assert.Equal("ok", database.GetProperty("data").GetProperty("connectivity").GetString());
            Assert.Equal(
                "current",
                database.GetProperty("data").GetProperty("migrationState").GetString());
            Assert.Equal(
                0,
                database.GetProperty("data").GetProperty("pendingMigrationCount").GetInt32());
        }
        finally
        {
            factory.Dispose();
            DeleteDatabase(databasePath);
        }
    }

    [Fact]
    public async Task PendingMigrations_ReturnServiceUnavailableWithMigrationState()
    {
        var databasePath = CreateDatabasePath();
        File.WriteAllBytes(databasePath, Array.Empty<byte>());
        var factory = new DatabaseReadinessFactory(databasePath);

        try
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("/health/ready");
            using var document = await ParseResponseAsync(response);
            var database = document.RootElement
                .GetProperty("checks")
                .GetProperty("database");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal("Unhealthy", document.RootElement.GetProperty("status").GetString());
            Assert.Equal("Unhealthy", database.GetProperty("status").GetString());
            Assert.Equal("ok", database.GetProperty("data").GetProperty("connectivity").GetString());
            Assert.Equal(
                "pending",
                database.GetProperty("data").GetProperty("migrationState").GetString());
            Assert.True(
                database.GetProperty("data").GetProperty("pendingMigrationCount").GetInt32() > 0);
        }
        finally
        {
            factory.Dispose();
            DeleteDatabase(databasePath);
        }
    }

    [Fact]
    public async Task UnavailableDatabase_ReturnServiceUnavailableWithoutSensitiveDetails()
    {
        var databaseDirectory = Path.Combine(
            Path.GetTempPath(),
            $"mediavault-health-{Guid.NewGuid():N}");
        var databasePath = Path.Combine(databaseDirectory, "mediavault.db");
        var factory = new DatabaseReadinessFactory(databasePath);

        try
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("/health/ready");
            using var document = await ParseResponseAsync(response);
            var database = document.RootElement
                .GetProperty("checks")
                .GetProperty("database");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal("Unhealthy", document.RootElement.GetProperty("status").GetString());
            Assert.Equal(
                "failed",
                database.GetProperty("data").GetProperty("connectivity").GetString());
            Assert.Equal(
                "unknown",
                database.GetProperty("data").GetProperty("migrationState").GetString());
            Assert.DoesNotContain(databasePath, document.RootElement.GetRawText());
        }
        finally
        {
            factory.Dispose();
            if (Directory.Exists(databaseDirectory))
                Directory.Delete(databaseDirectory, recursive: true);
        }
    }

    private static async Task<JsonDocument> ParseResponseAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }

    private static string CreateDatabasePath() =>
        Path.Combine(
            Path.GetTempPath(),
            $"mediavault-health-{Guid.NewGuid():N}.db");

    private static void DeleteDatabase(string databasePath)
    {
        if (File.Exists(databasePath))
            File.Delete(databasePath);

        if (File.Exists($"{databasePath}-wal"))
            File.Delete($"{databasePath}-wal");

        if (File.Exists($"{databasePath}-shm"))
            File.Delete($"{databasePath}-shm");
    }

    private sealed class DatabaseReadinessFactory(string databasePath)
        : WebApplicationFactory<media_vault_app.API.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Staging");
            builder.UseSetting(
                "ConnectionStrings:Default",
                $"Data Source={databasePath};Pooling=False");
            builder.UseSetting("ExternalApis:Rawg:BaseUrl", "https://rawg.test/");
            builder.UseSetting("ExternalApis:Rawg:ApiKey", "test-key");
            builder.UseSetting("ExternalApis:Tmdb:BaseUrl", "https://tmdb.test/");
            builder.UseSetting("ExternalApis:Tmdb:ApiAccessToken", "test-token");
            builder.UseSetting("ExternalApis:GoogleBooks:BaseUrl", "https://books.test/");
            builder.UseSetting("ExternalApis:GoogleBooks:ApiKey", "test-key");
            builder.UseSetting("Jwt:SecretKey", "integration-test-signing-key-at-least-32-bytes");
            builder.UseSetting("Jwt:Issuer", "MediaVault.Tests");
            builder.UseSetting("Jwt:Audience", "MediaVault.Tests");
        }
    }
}
