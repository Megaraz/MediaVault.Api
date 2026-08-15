using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        Assert.True(paths.TryGetProperty("/Auth/register", out _));
        Assert.True(paths.TryGetProperty("/Auth/me", out _));
        Assert.False(paths.TryGetProperty("/Users", out _));

        var registerResponses = paths
            .GetProperty("/Auth/register")
            .GetProperty("post")
            .GetProperty("responses");
        AssertSchema(registerResponses, "413", "ErrorResponseBody");

        var collectionOperation = paths
            .GetProperty("/MediaEntries")
            .GetProperty("get");
        var responses = collectionOperation.GetProperty("responses");

        Assert.True(responses.TryGetProperty("200", out _));
        AssertSchema(
            responses,
            "401",
            "MediaVaultAuthorizationProblemDetails",
            "application/problem+json");
        AssertSchema(
            responses,
            "403",
            "MediaVaultAuthorizationProblemDetails",
            "application/problem+json");
        AssertSchema(responses, "404", "ErrorResponseBody");
        AssertSchema(responses, "422", "ValidationErrorResponseBody");
        AssertSchema(responses, "503", "ErrorResponseBody");

        var createMovieResponses = paths
            .GetProperty("/MediaEntries/movies")
            .GetProperty("post")
            .GetProperty("responses");
        AssertSchema(createMovieResponses, "413", "ErrorResponseBody");

        var resilienceResponses = new[]
        {
            paths.GetProperty("/Auth/login").GetProperty("post").GetProperty("responses"),
            paths.GetProperty("/RawgApi/{id}").GetProperty("get").GetProperty("responses"),
            paths.GetProperty("/TmdbApi/movie/{id}").GetProperty("get").GetProperty("responses"),
            paths.GetProperty("/GoogleBooksApi/{volumeId}").GetProperty("get").GetProperty("responses")
        };
        foreach (var resilienceResponse in resilienceResponses)
        {
            AssertSchema(resilienceResponse, "429", "ErrorResponseBody");
            AssertSchema(resilienceResponse, "504", "ErrorResponseBody");
        }
    }

    [Fact]
    public async Task ProtectedSurface_RequiresAuthenticationAndHasNoUserManagementRoutes()
    {
        await using var factory = new OpenApiFactory();
        using var client = factory.CreateClient();

        var fallbackPolicy = factory.Services
            .GetRequiredService<IOptions<AuthorizationOptions>>()
            .Value
            .FallbackPolicy;

        Assert.NotNull(fallbackPolicy);
        Assert.Contains(
            fallbackPolicy.Requirements,
            requirement => requirement is DenyAnonymousAuthorizationRequirement);

        using var currentUserResponse = await client.GetAsync("/Auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, currentUserResponse.StatusCode);

        using var listUsersResponse = await client.GetAsync("/Users");
        Assert.Equal(HttpStatusCode.Unauthorized, listUsersResponse.StatusCode);

        using var getUserResponse = await client.GetAsync(
            $"/Users/{Guid.Empty}");
        Assert.Equal(HttpStatusCode.Unauthorized, getUserResponse.StatusCode);

        using var deleteUserResponse = await client.DeleteAsync(
            $"/Users/{Guid.Empty}");
        Assert.Equal(HttpStatusCode.Unauthorized, deleteUserResponse.StatusCode);
    }

    private static void AssertSchema(
        JsonElement responses,
        string status,
        string schemaName,
        string contentType = "application/json")
    {
        var content = responses
            .GetProperty(status)
            .GetProperty("content");
        Assert.Contains(
            contentType,
            content.EnumerateObject().Select(item => item.Name));

        var reference = content
            .GetProperty(contentType)
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
