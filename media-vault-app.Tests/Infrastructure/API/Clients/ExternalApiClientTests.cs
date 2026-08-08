using System.Net;
using System.Text;
using media_vault_app.API.Controllers;
using media_vault_app.Domain.Enums;
using media_vault_app.Infrastructure.API.Clients;
using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Megaraz.ResultPattern.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Rasmus.SharedKernel.ExternalServices;
using ApiErrorResponseBody = media_vault_app.API.Controllers.ErrorResponseBody;

namespace media_vault_app.Tests.Infrastructure.API.Clients;

public class ExternalApiClientTests
{
    private static readonly ErrorContext ErrorContext =
        new(OperationType.GetCollection, "External media");

    [Fact]
    public async Task ProviderClients_DeserializeRepresentativeResponsesThroughSharedMapping()
    {
        using var logger = new RecordingLoggerProvider();

        using var googleHttpClient = CreateHttpClient(
            JsonResponse(HttpStatusCode.OK, """{"id":"book-1","volumeInfo":{"title":"Book"}}"""));
        var googleClient = new GoogleBooksApiClient(
            googleHttpClient,
            Options.Create(new GoogleBooksApiOptions { BaseUrl = "https://books.test/", ApiKey = "secret" }),
            CreateErrorEventLogger<GoogleBooksApiClient>(logger));

        using var rawgHttpClient = CreateHttpClient(
            JsonResponse(HttpStatusCode.OK, """{"results":[]}"""));
        var rawgClient = new RawgApiClient(
            rawgHttpClient,
            Options.Create(new RawgApiOptions { BaseUrl = "https://rawg.test/", ApiKey = "secret" }),
            CreateErrorEventLogger<RawgApiClient>(logger));

        using var tmdbHttpClient = CreateHttpClient(
            JsonResponse(HttpStatusCode.OK, """{"page":1,"total_pages":1,"total_results":0,"results":[]}"""));
        var tmdbClient = new TmdbApiClient(
            tmdbHttpClient,
            Options.Create(new TmdbApiOptions { BaseUrl = "https://tmdb.test/", ApiAccessToken = "secret" }),
            CreateErrorEventLogger<TmdbApiClient>(logger));

        var googleResult = await googleClient.GetBookByIdAsync("book-1");
        var rawgResult = await rawgClient.SearchGamesAsync(["search=game"]);
        var tmdbResult = await tmdbClient.SearchAsync(["query=movie"], MediaType.Movie);

        Assert.Equal("book-1", googleResult.Value.Id);
        Assert.Empty(rawgResult.Value.Results!);
        Assert.Empty(tmdbResult.Value.Results!);
        Assert.Empty(logger.Entries);
    }

    [Fact]
    public async Task SharedMapping_PreservesWebJsonDefaultsAndMissingContentType()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes("""{"displayName":"Ada"}"""))
        };
        var client = CreateTestClient(response);

        var result = await client.GetAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("Ada", result.Value.DisplayName);
    }

    [Fact]
    public async Task SharedMapping_RejectsNonJsonSuccessContentTypeWithSafeMessage()
    {
        using var response = TextResponse(HttpStatusCode.OK, """{"displayName":"Ada"}""");
        var client = CreateTestClient(response);

        var result = await client.GetAsync();

        var error = Assert.IsType<HttpError>(result.PrimaryError);
        Assert.Equal(HttpErrorType.MalformedResponse, error.HttpErrorType);
        Assert.Equal(ExternalServiceResponsePolicy.GetSafeUserMessage(HttpStatusCode.OK), result.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-json")]
    public async Task SharedMapping_ReturnsSafeMalformedResponseForInvalidSuccessBodies(string? body)
    {
        using var response = JsonResponse(HttpStatusCode.OK, body);
        using var logger = new RecordingLoggerProvider();
        var client = CreateTestClient(response, logger);

        var result = await client.GetAsync();

        var error = Assert.IsType<HttpError>(result.PrimaryError);
        Assert.Equal(HttpErrorType.MalformedResponse, error.HttpErrorType);
        Assert.Equal(ExternalServiceResponsePolicy.GetSafeUserMessage(HttpStatusCode.OK), result.Message);
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(2102, entry.EventId.Id);
        Assert.Equal("ExternalDependencyInvalidResponse", entry.EventId.Name);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, HttpErrorType.BadRequest, 400)]
    [InlineData(HttpStatusCode.Unauthorized, HttpErrorType.Unauthorized, 401)]
    [InlineData(HttpStatusCode.Forbidden, HttpErrorType.Forbidden, 403)]
    [InlineData(HttpStatusCode.NotFound, HttpErrorType.NotFound, 404)]
    [InlineData(HttpStatusCode.Conflict, HttpErrorType.Conflict, 409)]
    [InlineData(HttpStatusCode.UnprocessableContent, HttpErrorType.UnprocessableContent, 422)]
    [InlineData(HttpStatusCode.TooManyRequests, HttpErrorType.TooManyRequests, 429)]
    [InlineData(HttpStatusCode.InternalServerError, HttpErrorType.InternalServerError, 502)]
    [InlineData((HttpStatusCode)418, HttpErrorType.UnexpectedStatusCode, 502)]
    public async Task SharedMapping_UsesFixedMessagesWithoutExposingProviderErrors(
        HttpStatusCode statusCode,
        HttpErrorType expectedType,
        int expectedApiStatusCode)
    {
        const string upstreamText = "raw-provider-detail-must-remain-private";
        using var response = JsonResponse(statusCode, $$"""{"message":"{{upstreamText}}"}""");
        var client = CreateTestClient(response);

        var result = await client.GetAsync();

        var error = Assert.IsType<HttpError>(result.PrimaryError);
        Assert.Equal(expectedType, error.HttpErrorType);
        Assert.Equal(ExternalServiceResponsePolicy.GetSafeUserMessage(statusCode), result.Message);
        Assert.DoesNotContain(upstreamText, result.Message, StringComparison.Ordinal);
        Assert.Contains(upstreamText, error.Description, StringComparison.Ordinal);

        var action = ResultResponseMapper.ToActionResult(new TestController(), result);
        var objectResult = Assert.IsType<ObjectResult>(action.Result);
        Assert.Equal(expectedApiStatusCode, objectResult.StatusCode);
        var body = Assert.IsType<ApiErrorResponseBody>(objectResult.Value);
        Assert.Equal(result.Message, body.Message);
        Assert.DoesNotContain(upstreamText, body.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, null)]
    [InlineData(HttpStatusCode.NotFound, null)]
    [InlineData(HttpStatusCode.Unauthorized, 2101)]
    [InlineData(HttpStatusCode.Forbidden, 2101)]
    [InlineData(HttpStatusCode.TooManyRequests, 2100)]
    [InlineData(HttpStatusCode.InternalServerError, 2100)]
    [InlineData((HttpStatusCode)418, 2102)]
    public async Task SharedMapping_EmitsAtMostOnePolicyEventForUpstreamHttpFailure(
        HttpStatusCode statusCode,
        int? expectedEventId)
    {
        const string upstreamText = "private-upstream-body";
        using var provider = new RecordingLoggerProvider();
        using var response = JsonResponse(statusCode, $$"""{"message":"{{upstreamText}}"}""");
        var client = CreateTestClient(response, provider);

        var result = await client.GetAsync();

        Assert.True(result.IsFailure);
        if (expectedEventId is null)
        {
            Assert.Empty(provider.Entries);
            return;
        }

        var entry = Assert.Single(provider.Entries);
        Assert.Equal(expectedEventId, entry.EventId.Id);
        Assert.Equal((int)statusCode, entry.Properties["StatusCode"]);
        Assert.DoesNotContain(upstreamText, entry.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(upstreamText, entry.Properties.Values.OfType<string>());
    }

    [Fact]
    public async Task SharedMapping_AcceptsSuccessBodyAtExactTwoMiBBoundary()
    {
        var body = CreatePayloadJson(ExternalServiceResponsePolicy.MaxInspectedBodyBytes);
        using var response = JsonResponse(HttpStatusCode.OK, body);
        var client = CreateTestClient(response);

        var result = await client.GetAsync();

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Value);
    }

    [Fact]
    public async Task SharedMapping_RejectsSuccessBodyOneByteOverTwoMiBBoundary()
    {
        var body = CreatePayloadJson(ExternalServiceResponsePolicy.MaxInspectedBodyBytes + 1);
        using var response = JsonResponse(HttpStatusCode.OK, body);
        var client = CreateTestClient(response);

        var result = await client.GetAsync();

        var error = Assert.IsType<HttpError>(result.PrimaryError);
        Assert.Equal(HttpErrorType.MalformedResponse, error.HttpErrorType);
        Assert.Contains("too large", error.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SharedMapping_InspectsErrorAtBoundaryButNotOneByteOver()
    {
        const string marker = "bounded-private-provider-detail";
        var atBoundaryBody = marker.PadRight(ExternalServiceResponsePolicy.MaxInspectedBodyBytes, 'x');
        var oversizedBody = marker.PadRight(ExternalServiceResponsePolicy.MaxInspectedBodyBytes + 1, 'x');
        using var atBoundaryResponse = TextResponse(HttpStatusCode.BadRequest, atBoundaryBody);
        using var oversizedResponse = TextResponse(HttpStatusCode.BadRequest, oversizedBody);
        var atBoundaryClient = CreateTestClient(atBoundaryResponse);
        var oversizedClient = CreateTestClient(oversizedResponse);

        var atBoundaryResult = await atBoundaryClient.GetAsync();
        var oversizedResult = await oversizedClient.GetAsync();

        Assert.Contains(marker, atBoundaryResult.PrimaryError.Description, StringComparison.Ordinal);
        Assert.DoesNotContain(marker, oversizedResult.PrimaryError.Description, StringComparison.Ordinal);
        Assert.DoesNotContain(marker, atBoundaryResult.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(marker, oversizedResult.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SharedMapping_PropagatesCallerCancellationWithoutLogging()
    {
        using var logger = new RecordingLoggerProvider();
        var client = new TestApiClient(
            _ => throw new TaskCanceledException("caller cancelled"),
            CreateErrorEventLogger<TestApiClient>(logger));
        using var source = new CancellationTokenSource();
        source.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetAsync(source.Token));

        Assert.Equal(source.Token, exception.CancellationToken);
        Assert.Empty(logger.Entries);
    }

    [Theory]
    [MemberData(nameof(TransportExceptions))]
    public async Task SharedMapping_MapsAndLogsNonCallerTransportFailuresOnce(Exception exception)
    {
        using var logger = new RecordingLoggerProvider();
        var client = new TestApiClient(
            _ => throw exception,
            CreateErrorEventLogger<TestApiClient>(logger));

        var result = await client.GetAsync();

        var error = Assert.IsType<HttpError>(result.PrimaryError);
        Assert.Equal(HttpErrorType.TransportFailure, error.HttpErrorType);
        Assert.Same(exception, error.Exception);
        Assert.Equal(ExternalServiceResponsePolicy.TransportFailureMessage, result.Message);
        Assert.DoesNotContain(exception.Message, result.Message, StringComparison.Ordinal);
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(2100, entry.EventId.Id);
        Assert.Equal("ExternalDependencyTransientFailure", entry.EventId.Name);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal("Infrastructure", entry.Properties["Layer"]);
        Assert.Equal(nameof(TestApiClient), entry.Properties["Service"]);
        Assert.Equal(nameof(TestApiClient.GetAsync), entry.Properties["Method"]);
        Assert.Equal("TestProvider", entry.Properties["Provider"]);
        Assert.Equal(HttpErrorType.TransportFailure.ToString(), entry.Properties["FailureKind"]);
        Assert.Equal(error.Code, entry.Properties["ErrorCode"]);
        Assert.Null(entry.Exception);
        Assert.DoesNotContain(exception.Message, entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void PackageTransportFailure_RetainsMediaVaultHttp503Contract()
    {
        var error = HttpError.TransportFailure(
            ErrorContext,
            new TimeoutException("private timeout detail"));
        var result = Result<TestPayload>.Failure(
            error,
            ExternalServiceResponsePolicy.TransportFailureMessage);

        var action = ResultResponseMapper.ToActionResult(new TestController(), result);

        var objectResult = Assert.IsType<ObjectResult>(action.Result);
        Assert.Equal(503, objectResult.StatusCode);
        var body = Assert.IsType<ApiErrorResponseBody>(objectResult.Value);
        Assert.Equal(ExternalServiceResponsePolicy.TransportFailureMessage, body.Message);
        Assert.DoesNotContain("private timeout detail", body.Message, StringComparison.Ordinal);
    }

    public static IEnumerable<object[]> TransportExceptions()
    {
        yield return [new HttpRequestException("transport detail")];
        yield return [new TimeoutException("timeout detail")];
        yield return [new TaskCanceledException("non-caller timeout detail")];
    }

    private static TestApiClient CreateTestClient(
        HttpResponseMessage response,
        RecordingLoggerProvider? logger = null) =>
        new(_ => Task.FromResult(response), CreateErrorEventLogger<TestApiClient>(logger));

    private static ErrorEventLogger<TCategory> CreateErrorEventLogger<TCategory>(
        RecordingLoggerProvider? provider = null)
        where TCategory : class
    {
        return new ErrorEventLogger<TCategory>(
            provider?.CreateLogger<TCategory>() ?? NullLogger<TCategory>.Instance,
            new ErrorEventPolicy(),
            new ErrorDiagnosticsOptions(false));
    }

    private static HttpClient CreateHttpClient(HttpResponseMessage response) =>
        new(new StubHttpMessageHandler((_, _) => Task.FromResult(response)))
        {
            BaseAddress = new Uri("https://provider.test/")
        };

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string? body) =>
        new(statusCode)
        {
            Content = body is null ? null : new StringContent(body, Encoding.UTF8, "application/json")
        };

    private static HttpResponseMessage TextResponse(HttpStatusCode statusCode, string body) =>
        new(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "text/plain")
        };

    private static string CreatePayloadJson(int byteCount)
    {
        const string prefix = "{\"displayName\":\"Ada\",\"value\":\"";
        const string suffix = "\"}";
        var valueLength = byteCount - Encoding.UTF8.GetByteCount(prefix + suffix);
        var json = prefix + new string('x', valueLength) + suffix;

        Assert.Equal(byteCount, Encoding.UTF8.GetByteCount(json));
        return json;
    }

    private sealed record TestPayload(string DisplayName = "", string Value = "");

    private sealed class TestApiClient : ApiClientBase<TestApiClient>
    {
        private readonly Func<CancellationToken, Task<HttpResponseMessage>> _sendAsync;

        public TestApiClient(
            Func<CancellationToken, Task<HttpResponseMessage>> sendAsync,
            ErrorEventLogger<TestApiClient> errorEventLogger)
            : base(errorEventLogger, "TestProvider")
        {
            _sendAsync = sendAsync;
        }

        public Task<Result<TestPayload>> GetAsync(CancellationToken cancellationToken = default) =>
            SendAndMapAsync<TestPayload>(_sendAsync, ErrorContext, cancellationToken);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            sendAsync(request, cancellationToken);
    }

    private sealed class TestController : ControllerBase;

}
