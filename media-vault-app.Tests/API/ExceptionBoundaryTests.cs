using System.Net;
using System.Text.Json;
using media_vault_app.API.Controllers;
using media_vault_app.API.Diagnostics;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace media_vault_app.Tests.API;

public sealed class ExceptionBoundaryTests
{
    private const string TraceId = "0123456789abcdef0123456789abcdef";
    private const string PrivateDetail = "private SQL password=super-secret upstream-body";

    [Theory]
    [InlineData("Development", true)]
    [InlineData("Staging", false)]
    public async Task UnhandledException_ReturnsSafeCorrelatedProblemDetailsAndOneOwnedEvent(
        string environment,
        bool expectsLocalException)
    {
        await using var factory = new ExceptionBoundaryFactory(environment);
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/_test/exception-boundary/throw");
        request.Headers.TryAddWithoutValidation(
            "traceparent",
            $"00-{TraceId}-0123456789abcdef-01");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(
            ["type", "title", "status", "detail", "traceId"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal("An unexpected error occurred.", root.GetProperty("title").GetString());
        Assert.Equal(500, root.GetProperty("status").GetInt32());
        Assert.Equal("The server could not complete the request.", root.GetProperty("detail").GetString());
        var responseTraceId = root.GetProperty("traceId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(responseTraceId));
        Assert.Contains(TraceId, responseTraceId, StringComparison.Ordinal);
        Assert.DoesNotContain(PrivateDetail, json, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), json, StringComparison.Ordinal);

        var entry = Assert.Single(factory.Logs.Entries, entry => entry.EventId.Id == 3000);
        Assert.Same(entry, Assert.Single(factory.Logs.Entries, entry => entry.Level == LogLevel.Error));
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal("UnhandledRequestException", entry.EventId.Name);
        Assert.Equal("API", entry.Properties["Layer"]);
        Assert.Equal("MediaVaultExceptionHandler", entry.Properties["Service"]);
        Assert.Equal("TryHandleAsync", entry.Properties["Method"]);
        Assert.Equal(typeof(InvalidOperationException).FullName, entry.Properties["ExceptionType"]);
        Assert.Equal(responseTraceId, entry.Properties["TraceId"]);
        Assert.Equal(expectsLocalException, entry.Exception is InvalidOperationException);
        Assert.DoesNotContain(PrivateDetail, entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExpectedResultAndAuthenticationChallenge_KeepExistingContractsWithoutUnhandledEvent()
    {
        await using var factory = new ExceptionBoundaryFactory(Environments.Staging);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var expectedResponse = await client.GetAsync("/_test/exception-boundary/expected");
        var expectedJson = await expectedResponse.Content.ReadAsStringAsync();
        using var expectedDocument = JsonDocument.Parse(expectedJson);

        Assert.Equal(HttpStatusCode.NotFound, expectedResponse.StatusCode);
        Assert.Equal(
            ["message", "code"],
            expectedDocument.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal("The requested test resource was not found.", expectedDocument.RootElement.GetProperty("message").GetString());

        using var challengeResponse = await client.GetAsync("/MediaEntries");

        Assert.Equal(HttpStatusCode.Unauthorized, challengeResponse.StatusCode);
        Assert.DoesNotContain(factory.Logs.Entries, entry => entry.EventId.Id == 3000);
        Assert.DoesNotContain(factory.Logs.Entries, entry => entry.Level == LogLevel.Error);
    }

    [Fact]
    public async Task CallerCancellation_IsNotConvertedToOrLoggedAsGeneric500()
    {
        await using var factory = new ExceptionBoundaryFactory(Environments.Staging);
        using var client = factory.CreateClient();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GetAsync("/_test/exception-boundary/cancel", cancellation.Token));

        Assert.DoesNotContain(factory.Logs.Entries, entry => entry.EventId.Id == 3000);
        Assert.DoesNotContain(factory.Logs.Entries, entry => entry.EventId.Id == 3001);
        Assert.DoesNotContain(factory.Logs.Entries, entry => entry.Level == LogLevel.Error);
    }

    [Fact]
    public async Task ServerBudgetExpiry_CancelsWorkAndWritesOneSafeTimeoutResponse()
    {
        await using var factory = new ExceptionBoundaryFactory(Environments.Staging);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/_test/exception-boundary/request-timeout");

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("The request timed out. Please try again.", json.RootElement.GetProperty("message").GetString());
        Assert.Equal("Request.Timeout", json.RootElement.GetProperty("code").GetString());
        Assert.Single(factory.Logs.Entries, entry => entry.EventId.Id == 3001);
        Assert.DoesNotContain(factory.Logs.Entries, entry => entry.EventId.Id == 3000);
    }

    private sealed class ExceptionBoundaryFactory(string environment)
        : WebApplicationFactory<media_vault_app.API.Program>
    {
        public RecordingLoggerProvider Logs { get; } = new();

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
            builder.UseSetting("RequestTimeouts:AuthenticationMilliseconds", "50");
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddProvider(Logs);
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IHostedService>();
                services.AddControllers().AddApplicationPart(typeof(ExceptionBoundaryTestController).Assembly);
            });
        }
    }
}

[ApiController]
[AllowAnonymous]
[Route("_test/exception-boundary")]
public sealed class ExceptionBoundaryTestController : ControllerBase
{
    [HttpGet("throw")]
    public IActionResult Throw() =>
        throw new InvalidOperationException("private SQL password=super-secret upstream-body");

    [HttpGet("expected")]
    public ActionResult<int> Expected()
    {
        var context = new ErrorContext(OperationType.Get, "TestResource");
        var result = Result<int>.Failure(
            Error.NotFound(context),
            "The requested test resource was not found.");
        return this.ToActionResult(result);
    }

    [HttpGet("cancel")]
    public async Task<IActionResult> Cancel(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        return Ok();
    }

    [HttpGet("request-timeout")]
    [RequestTimeout(MediaVaultRequestTimeoutPolicies.Authentication)]
    public async Task<IActionResult> RequestTimeout(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        return Ok();
    }
}
