using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using media_vault_app.API.RateLimiting;
using media_vault_app.API.Security;
using media_vault_app.Tests.TestHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace media_vault_app.Tests.API;

public sealed class RateLimitingTests
{
    [Fact]
    public async Task Rejection_HasStableBodyJsonAndRetryAfter_AndDoesNotTrustForwardedIp()
    {
        await using var factory = new RateLimitingFactory();
        using var client = factory.CreateClient();

        using var first = new HttpRequestMessage(HttpMethod.Get, "/_test/rate-limit/login");
        first.Headers.TryAddWithoutValidation("X-Forwarded-For", "198.51.100.10");
        using var firstResponse = await client.SendAsync(first);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        using var rejected = new HttpRequestMessage(HttpMethod.Get, "/_test/rate-limit/login");
        rejected.Headers.TryAddWithoutValidation("X-Forwarded-For", "203.0.113.20");
        using var response = await client.SendAsync(rejected);

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(response.Headers.RetryAfter);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Too many requests. Please try again later.", body.RootElement.GetProperty("message").GetString());
        Assert.Equal("Request.RateLimited", body.RootElement.GetProperty("code").GetString());
        Assert.Single(factory.Logs.Entries, entry => entry.EventId.Id == 3002);
    }

    [Fact]
    public async Task AuthenticatedMetadataPolicies_PartitionByValidatedUser_AndLeaveUnmarkedEndpointsUntouched()
    {
        await using var factory = new RateLimitingFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var tokens = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var userOne = tokens.GenerateToken(Guid.NewGuid(), "first", "first@example.test");
        var userTwo = tokens.GenerateToken(Guid.NewGuid(), "second", "second@example.test");

        Assert.Equal(HttpStatusCode.OK, (await SendAuthorized(client, "/_test/rate-limit/metadata", userOne)).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await SendAuthorized(client, "/_test/rate-limit/metadata", userOne)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SendAuthorized(client, "/_test/rate-limit/metadata", userTwo)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/_test/rate-limit/ordinary")).StatusCode);
    }

    [Fact]
    public void Options_RejectQueuesAndInvalidDurations()
    {
        var invalid = new FixedWindowRateLimitOptions { PermitLimit = 0, WindowSeconds = 0, QueueLimit = 1 };
        var results = invalid.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(invalid)).ToArray();

        Assert.Equal(3, results.Length);
    }

    private static async Task<HttpResponseMessage> SendAuthorized(HttpClient client, string path, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path)
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };
        return await client.SendAsync(request);
    }

    private sealed class RateLimitingFactory : WebApplicationFactory<media_vault_app.API.Program>
    {
        public RecordingLoggerProvider Logs { get; } = new();

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
            builder.UseSetting("Jwt:SecretKey", "integration-test-signing-key-at-least-32-bytes");
            builder.UseSetting("Jwt:Issuer", "MediaVault.Tests");
            builder.UseSetting("Jwt:Audience", "MediaVault.Tests");
            builder.UseSetting("RateLimiting:LoginByIp:PermitLimit", "1");
            builder.UseSetting("RateLimiting:LoginByIp:WindowSeconds", "60");
            builder.UseSetting("RateLimiting:RegistrationByIp:PermitLimit", "1");
            builder.UseSetting("RateLimiting:RawgMetadataByUser:PermitLimit", "1");
            builder.UseSetting("RateLimiting:TmdbMetadataByUser:TokenLimit", "1");
            builder.UseSetting("RateLimiting:TmdbMetadataByUser:TokensPerPeriod", "1");
            builder.UseSetting("RateLimiting:GoogleBooksMetadataByUser:PermitLimit", "1");
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(Logs);
            });
            builder.ConfigureServices(services =>
                services.AddControllers().AddApplicationPart(typeof(RateLimitingTestController).Assembly));
        }
    }
}

[ApiController]
[Route("_test/rate-limit")]
public sealed class RateLimitingTestController : ControllerBase
{
    [HttpGet("login")]
    [AllowAnonymous]
    [EnableRateLimiting(MediaVaultRateLimitPolicies.LoginByIp)]
    public IActionResult Login() => Ok();

    [Authorize]
    [HttpGet("metadata")]
    [EnableRateLimiting(MediaVaultRateLimitPolicies.RawgMetadataByUser)]
    public IActionResult Metadata() => Ok();

    [HttpGet("ordinary")]
    [AllowAnonymous]
    public IActionResult Ordinary() => Ok();
}
