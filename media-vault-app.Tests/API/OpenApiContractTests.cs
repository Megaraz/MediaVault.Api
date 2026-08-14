using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace media_vault_app.Tests.API;

public sealed class OpenApiContractTests
{
    [Fact]
    public async Task GeneratedDocument_PreservesRepresentativeResultContracts()
    {
        await using var factory = new OpenApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var paths = document.RootElement.GetProperty("paths");
        var collectionOperation = paths
            .GetProperty("/MediaEntries")
            .GetProperty("get");
        var responses = collectionOperation.GetProperty("responses");

        Assert.True(responses.TryGetProperty("200", out _));
        AssertSchema(responses, "404", "ErrorResponseBody");
        AssertSchema(responses, "422", "ValidationErrorResponseBody");
        AssertSchema(responses, "503", "ErrorResponseBody");

        AssertSchema(
            paths.GetProperty("/Auth/login").GetProperty("post").GetProperty("responses"),
            "429",
            "ErrorResponseBody");
        AssertSchema(
            paths.GetProperty("/RawgApi/{id}").GetProperty("get").GetProperty("responses"),
            "429",
            "ErrorResponseBody");
        AssertSchema(
            paths.GetProperty("/TmdbApi/movie/{id}").GetProperty("get").GetProperty("responses"),
            "429",
            "ErrorResponseBody");
        AssertSchema(
            paths.GetProperty("/GoogleBooksApi/{volumeId}").GetProperty("get").GetProperty("responses"),
            "429",
            "ErrorResponseBody");
    }

    private static void AssertSchema(
        JsonElement responses,
        string status,
        string schemaName)
    {
        var content = responses
            .GetProperty(status)
            .GetProperty("content");
        Assert.Contains(
            "application/json",
            content.EnumerateObject().Select(item => item.Name));

        var reference = content
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();
        Assert.EndsWith($"/{schemaName}", reference, StringComparison.Ordinal);
    }

    private sealed class OpenApiFactory
        : WebApplicationFactory<media_vault_app.API.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:Default", "Data Source=:memory:");
            builder.UseSetting("ExternalApis:Rawg:BaseUrl", "https://rawg.test/");
            builder.UseSetting("ExternalApis:Rawg:ApiKey", "test-key");
            builder.UseSetting("ExternalApis:Tmdb:BaseUrl", "https://tmdb.test/");
            builder.UseSetting("ExternalApis:Tmdb:ApiAccessToken", "test-token");
            builder.UseSetting("ExternalApis:GoogleBooks:BaseUrl", "https://books.test/");
            builder.UseSetting("ExternalApis:GoogleBooks:ApiKey", "test-key");
            builder.UseSetting(
                "Jwt:SecretKey",
                "integration-test-signing-key-at-least-32-bytes");
            builder.UseSetting("Jwt:Issuer", "MediaVault.Tests");
            builder.UseSetting("Jwt:Audience", "MediaVault.Tests");
        }
    }
}
