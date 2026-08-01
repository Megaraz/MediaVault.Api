using System.Net;
using System.Text;
using media_vault_app.API.Controllers;
using media_vault_app.Domain.Enums;
using media_vault_app.Infrastructure.API.Clients;
using Megaraz.ResultPattern;
using Megaraz.ResultPattern.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rasmus.SharedKernel.Diagnostics;
using Rasmus.SharedKernel.ExternalServices;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;
using ErrorLog = Rasmus.SharedKernel.Diagnostics.ErrorLog;
using ApiErrorResponseBody = media_vault_app.API.Controllers.ErrorResponseBody;

namespace media_vault_app.Tests.Infrastructure.API.Clients;

public class ExternalApiClientTests
{
    private static readonly ErrorContext ErrorContext =
        new(OperationType.GetCollection, "External media");

    [Fact]
    public async Task ProviderClients_DeserializeRepresentativeResponsesThroughSharedMapping()
    {
        var logger = new RecordingErrorLogger();
        var policy = new AlwaysLogPolicy();

        using var googleHttpClient = CreateHttpClient(
            JsonResponse(HttpStatusCode.OK, """{"id":"book-1","volumeInfo":{"title":"Book"}}"""));
        var googleClient = new GoogleBooksApiClient(
            googleHttpClient,
            Options.Create(new GoogleBooksApiOptions { BaseUrl = "https://books.test/", ApiKey = "secret" }),
            logger,
            policy);

        using var rawgHttpClient = CreateHttpClient(
            JsonResponse(HttpStatusCode.OK, """{"results":[]}"""));
        var rawgClient = new RawgApiClient(
            rawgHttpClient,
            Options.Create(new RawgApiOptions { BaseUrl = "https://rawg.test/", ApiKey = "secret" }),
            logger,
            policy);

        using var tmdbHttpClient = CreateHttpClient(
            JsonResponse(HttpStatusCode.OK, """{"page":1,"total_pages":1,"total_results":0,"results":[]}"""));
        var tmdbClient = new TmdbApiClient(
            tmdbHttpClient,
            Options.Create(new TmdbApiOptions { BaseUrl = "https://tmdb.test/", ApiAccessToken = "secret" }),
            logger,
            policy);

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
        var logger = new RecordingErrorLogger();
        var client = CreateTestClient(response, logger);

        var result = await client.GetAsync();

        var error = Assert.IsType<HttpError>(result.PrimaryError);
        Assert.Equal(HttpErrorType.MalformedResponse, error.HttpErrorType);
        Assert.Equal(ExternalServiceResponsePolicy.GetSafeUserMessage(HttpStatusCode.OK), result.Message);
        Assert.Single(logger.Entries);
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
        var logger = new RecordingErrorLogger();
        var client = new TestApiClient(
            _ => throw new TaskCanceledException("caller cancelled"),
            logger,
            new AlwaysLogPolicy());
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
        var logger = new RecordingErrorLogger();
        var client = new TestApiClient(
            _ => throw exception,
            logger,
            new AlwaysLogPolicy());

        var result = await client.GetAsync();

        var error = Assert.IsType<HttpError>(result.PrimaryError);
        Assert.Equal(HttpErrorType.TransportFailure, error.HttpErrorType);
        Assert.Same(exception, error.Exception);
        Assert.Equal(ExternalServiceResponsePolicy.TransportFailureMessage, result.Message);
        Assert.DoesNotContain(exception.Message, result.Message, StringComparison.Ordinal);
        var entry = Assert.Single(logger.Entries);
        Assert.Same(error, entry.Error);
        Assert.Equal("Infrastructure", entry.Context.Layer);
        Assert.Equal(nameof(TestApiClient), entry.Context.Service);
        Assert.Equal(nameof(TestApiClient.GetAsync), entry.Context.Method);
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
        RecordingErrorLogger? logger = null) =>
        new(_ => Task.FromResult(response), logger ?? new RecordingErrorLogger(), new AlwaysLogPolicy());

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

    private sealed class TestApiClient : ApiClientBase
    {
        private readonly Func<CancellationToken, Task<HttpResponseMessage>> _sendAsync;

        public TestApiClient(
            Func<CancellationToken, Task<HttpResponseMessage>> sendAsync,
            IErrorLogger errorLogger,
            IErrorLogPolicy errorLogPolicy)
            : base(errorLogger, errorLogPolicy)
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

    private sealed class AlwaysLogPolicy : IErrorLogPolicy
    {
        public bool ShouldLog(Error error) => true;
    }

    private sealed class RecordingErrorLogger : IErrorLogger
    {
        public List<(Error Error, ErrorLogContext Context)> Entries { get; } = [];

        public Task CleanOldLogsAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task<IReadOnlyList<ErrorLog>> GetErrorLogsAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ErrorLog>>([]);

        public Task LogErrorToFileAsync(
            Error error,
            ErrorLogContext context,
            CancellationToken ct = default)
        {
            Entries.Add((error, context));
            return Task.CompletedTask;
        }
    }
}
