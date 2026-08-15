using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using media_vault_app.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace media_vault_app.Tests.API;

public sealed class AuthenticationContractTests
{
    private const string TraceId = "0123456789abcdef0123456789abcdef";

    [Fact]
    public async Task MissingBearerToken_ReturnsSafeCorrelatedProblemDetailsAndBearerChallenge()
    {
        await using var factory = new AuthenticationContractFactory();
        using var client = factory.CreateClient();
        using var request = CreateRequest("/MediaEntries", TraceId);

        using var response = await client.SendAsync(request);

        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.Unauthorized,
            TraceId,
            "Authentication required.",
            "A valid bearer token is required to access this resource.");
        var challenge = Assert.Single(response.Headers.WwwAuthenticate);
        Assert.Equal("Bearer", challenge.Scheme);
        Assert.Null(challenge.Parameter);
    }

    [Fact]
    public async Task InvalidBearerToken_ReturnsSafeCorrelatedProblemDetailsAndBearerChallenge()
    {
        await using var factory = new AuthenticationContractFactory();
        using var client = factory.CreateClient();
        using var request = CreateRequest("/MediaEntries", TraceId);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        using var response = await client.SendAsync(request);

        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.Unauthorized,
            TraceId,
            "Authentication required.",
            "A valid bearer token is required to access this resource.");
        var challenge = Assert.Single(response.Headers.WwwAuthenticate);
        Assert.Equal("Bearer", challenge.Scheme);
        Assert.Null(challenge.Parameter);
    }

    [Fact]
    public async Task AuthenticatedRequestWithoutRequiredRole_ReturnsSafeCorrelatedForbiddenProblemDetails()
    {
        await using var factory = new AuthenticationContractFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var token = tokenService.GenerateToken(
            Guid.NewGuid(),
            "test-user",
            "test@example.test");
        using var request = CreateRequest(
            "/_test/auth-contract/forbidden",
            "fedcba98765432100123456789abcdef");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);

        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.Forbidden,
            "fedcba98765432100123456789abcdef",
            "Forbidden.",
            "You do not have permission to access this resource.");
        Assert.False(response.Headers.Contains("WWW-Authenticate"));
    }

    private static HttpRequestMessage CreateRequest(string path, string traceId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.TryAddWithoutValidation(
            "traceparent",
            $"00-{traceId}-0123456789abcdef-01");
        return request;
    }

    private static async Task AssertProblemDetailsAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedTraceId,
        string expectedTitle,
        string expectedDetail)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(
            ["type", "title", "status", "detail", "traceId"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("type").GetString()));
        Assert.Equal(expectedTitle, root.GetProperty("title").GetString());
        Assert.Equal((int)expectedStatus, root.GetProperty("status").GetInt32());
        Assert.Equal(expectedDetail, root.GetProperty("detail").GetString());
        Assert.Contains(
            expectedTraceId,
            root.GetProperty("traceId").GetString(),
            StringComparison.Ordinal);
        Assert.DoesNotContain("invalid-token", json, StringComparison.Ordinal);
        Assert.DoesNotContain("test@example.test", json, StringComparison.Ordinal);
        Assert.DoesNotContain("/_test/", json, StringComparison.Ordinal);
    }

    private sealed class AuthenticationContractFactory
        : WebApplicationFactory<media_vault_app.API.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Staging");
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
            builder.ConfigureServices(services =>
                services.AddControllers()
                    .AddApplicationPart(typeof(AuthenticationContractTestController).Assembly));
        }
    }
}

[ApiController]
[Route("_test/auth-contract")]
public sealed class AuthenticationContractTestController : ControllerBase
{
    [Authorize(Roles = "test-role")]
    [HttpGet("forbidden")]
    public IActionResult Forbidden() => Ok();
}
