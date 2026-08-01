using System.Net;
using System.Text;
using System.Text.Json;
using Rasmus.SharedKernel.Diagnostics;
using Rasmus.SharedKernel.ExternalServices;
using LegacyDatabaseError = Rasmus.SharedKernel.ResultPatternCompatibility.DatabaseError;
using LegacyErrorResponseBody = Rasmus.SharedKernel.ResultPatternCompatibility.ErrorResponseBody;
using LegacyResult = Rasmus.SharedKernel.ResultPattern.Result;
using LegacyValidationErrorItem = Rasmus.SharedKernel.ResultPatternCompatibility.ValidationErrorItem;
using LegacyValidationErrorResponseBody = Rasmus.SharedKernel.ResultPatternCompatibility.ValidationErrorResponseBody;
using PackageDatabaseError = Megaraz.ResultPattern.Infrastructure.DatabaseError;
using PackageError = Megaraz.ResultPattern.Error;
using PackageErrorContext = Megaraz.ResultPattern.ErrorContext;
using PackageErrorType = Megaraz.ResultPattern.ErrorType;
using PackageHttpError = Megaraz.ResultPattern.AspNetCore.HttpError;
using PackageHttpErrorType = Megaraz.ResultPattern.AspNetCore.HttpErrorType;
using PackageHttpExtensions = Megaraz.ResultPattern.AspNetCore.HttpResponseToResultExtensions;
using PackageHttpMapper = Megaraz.ResultPattern.AspNetCore.HttpResultMapper;
using PackageHttpPolicy = Megaraz.ResultPattern.AspNetCore.HttpResultMappingPolicy;
using PackageHttpResponseOptions = Megaraz.ResultPattern.AspNetCore.HttpResponseMappingOptions;
using PackageOperationType = Megaraz.ResultPattern.OperationType;
using PackageResult = Megaraz.ResultPattern.Result;
using PackageValidationError = Megaraz.ResultPattern.ValidationError;

namespace Rasmus.SharedKernel.Tests.Result_Pattern;

public class ResultPatternCompatibilityBaseline_Tests
{
    private static readonly PackageErrorContext PackageContext =
        new(PackageOperationType.Get, "ExternalMedia");

    [Fact]
    public void LocalAndPackageResultTypes_CoexistUnderDistinctExplicitAliases()
    {
        var legacyResult = LegacyResult.Success();
        var packageResult = PackageResult.Success();

        Assert.True(legacyResult.IsSuccess);
        Assert.True(packageResult.IsSuccess);
        Assert.Equal("Rasmus.SharedKernel.ResultPattern.Result", legacyResult.GetType().FullName);
        Assert.Equal("Megaraz.ResultPattern.Result", packageResult.GetType().FullName);
        Assert.NotEqual(legacyResult.GetType().Assembly, packageResult.GetType().Assembly);
    }

    [Fact]
    public void DatabaseCodeFormat_RecordsTemporaryBridgeAndApprovedPackageNativeValues()
    {
        var packageContext = new PackageErrorContext(PackageOperationType.Update, "MediaEntry");

        var legacyError = LegacyDatabaseError.SaveChangesFailure(packageContext, new Exception("database"));
        var packageError = PackageDatabaseError.SaveChangesFailure(packageContext, new Exception("database"));

        Assert.Equal("Update.MediaEntry.DbSaveChangesFailure", legacyError.Code);
        Assert.Equal("Update.MediaEntry.DatabaseSaveChangesFailure", packageError.Code);
    }

    [Fact]
    public void ApprovedPackageMapping_MapsNonHttpExternalTo500()
    {
        var error = PackageError.Custom(
            "Get.MediaEntry.ExternalFailure",
            "A technical external failure.",
            PackageErrorType.External,
            "The request could not be completed.");
        var result = PackageResult.Failure(error);

        var response = PackageHttpMapper.ToHttpResponse(result, CreateApprovedHttpPolicy());

        Assert.Equal(500, response.StatusCode);
        var body = Assert.IsType<LegacyErrorResponseBody>(response.Body);
        Assert.Equal("The request could not be completed.", body.Message);
        Assert.Equal(error.Code, body.Code);
    }

    [Fact]
    public void ApprovedPackageMapping_PrioritizesConcreteHttpErrorMapping()
    {
        var error = PackageHttpError.InternalServerError(
            PackageContext,
            userMessage: "The external service encountered an internal error.");
        var result = PackageResult.Failure(error);

        var response = PackageHttpMapper.ToHttpResponse(result, CreateApprovedHttpPolicy());

        Assert.Equal(502, response.StatusCode);
    }

    [Fact]
    public void ApprovedPackageMapping_PreservesValidationShapeWithoutTopLevelCode()
    {
        var validationError = PackageValidationError.Required(
            new PackageErrorContext(PackageOperationType.Create, "User", "Email"),
            userMessage: "Email is required.");
        var result = PackageResult.ValidationFailure([validationError], "Validation failed.");

        var response = PackageHttpMapper.ToHttpResponse(result, CreateApprovedHttpPolicy());

        Assert.Equal(422, response.StatusCode);
        var body = Assert.IsType<LegacyValidationErrorResponseBody>(response.Body);
        var json = SerializeContract(body);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(["message", "validationErrors"], root.EnumerateObject().Select(x => x.Name));
        Assert.Equal("Validation failed.", root.GetProperty("message").GetString());
        var item = Assert.Single(root.GetProperty("validationErrors").EnumerateArray());
        Assert.Equal("Email", item.GetProperty("field").GetString());
        Assert.Equal("Email is required.", item.GetProperty("message").GetString());
        Assert.False(root.TryGetProperty("code", out _));
    }

    [Fact]
    public async Task ApprovedOutboundPolicy_RetainsBoundedUpstreamTextOnlyInDiagnostics()
    {
        const string upstreamText = "raw-provider-detail-must-remain-private";
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent($"{{\"message\":\"{upstreamText}\"}}", Encoding.UTF8, "application/json")
        };
        var options = CreateOutboundOptions(HttpStatusCode.BadRequest);

        var result = await PackageHttpExtensions.MapToResultAsync<PayloadDto>(
            response,
            PackageContext,
            options);

        Assert.True(result.IsFailure);
        Assert.Equal(ExternalServiceResponsePolicy.GetSafeUserMessage(HttpStatusCode.BadRequest), result.Message);
        Assert.DoesNotContain(upstreamText, result.Message, StringComparison.Ordinal);
        Assert.Contains(upstreamText, result.PrimaryError.Description, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(400, "The external service rejected the request.")]
    [InlineData(401, "The external service could not authenticate the request.")]
    [InlineData(403, "The external service refused the request.")]
    [InlineData(404, "The requested resource was not found in the external service.")]
    [InlineData(409, "The external service reported a conflict.")]
    [InlineData(422, "The external service could not process the request.")]
    [InlineData(429, "The external service is temporarily rate-limiting requests.")]
    [InlineData(500, "The external service encountered an internal error.")]
    [InlineData(502, "The external service returned an unexpected response.")]
    public void ApprovedOutboundPolicy_UsesFixedStatusSpecificUserMessages(
        int statusCode,
        string expectedMessage)
    {
        var message = ExternalServiceResponsePolicy.GetSafeUserMessage((HttpStatusCode)statusCode);

        Assert.Equal(expectedMessage, message);
    }

    [Fact]
    public async Task ApprovedOutboundPolicy_AcceptsSuccessBodyAtExactTwoMiBBoundary()
    {
        var json = CreatePayloadJson(ExternalServiceResponsePolicy.MaxInspectedBodyBytes);
        using var response = JsonResponse(HttpStatusCode.OK, json);

        var result = await PackageHttpExtensions.MapToResultAsync<PayloadDto>(
            response,
            PackageContext,
            CreateOutboundOptions(HttpStatusCode.OK));

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Value);
    }

    [Fact]
    public async Task ApprovedOutboundPolicy_RejectsSuccessBodyOneByteOverTwoMiBBoundary()
    {
        var json = CreatePayloadJson(ExternalServiceResponsePolicy.MaxInspectedBodyBytes + 1);
        using var response = JsonResponse(HttpStatusCode.OK, json);

        var result = await PackageHttpExtensions.MapToResultAsync<PayloadDto>(
            response,
            PackageContext,
            CreateOutboundOptions(HttpStatusCode.OK));

        Assert.True(result.IsFailure);
        var error = Assert.IsType<PackageHttpError>(result.PrimaryError);
        Assert.Equal(PackageHttpErrorType.MalformedResponse, error.HttpErrorType);
        Assert.Contains("too large", error.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApprovedOutboundPolicy_AcceptsErrorBodyAtExactTwoMiBBoundary()
    {
        const string marker = "bounded-private-provider-detail";
        var body = marker.PadRight(ExternalServiceResponsePolicy.MaxInspectedBodyBytes, 'x');
        using var response = TextResponse(HttpStatusCode.BadRequest, body);

        var result = await PackageHttpExtensions.MapToResultAsync<PayloadDto>(
            response,
            PackageContext,
            CreateOutboundOptions(HttpStatusCode.BadRequest));

        Assert.True(result.IsFailure);
        Assert.Contains(marker, result.PrimaryError.Description, StringComparison.Ordinal);
        Assert.DoesNotContain(marker, result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApprovedOutboundPolicy_DoesNotInspectErrorBodyOneByteOverTwoMiBBoundary()
    {
        const string marker = "oversized-private-provider-detail";
        var body = marker.PadRight(ExternalServiceResponsePolicy.MaxInspectedBodyBytes + 1, 'x');
        using var response = TextResponse(HttpStatusCode.BadRequest, body);

        var result = await PackageHttpExtensions.MapToResultAsync<PayloadDto>(
            response,
            PackageContext,
            CreateOutboundOptions(HttpStatusCode.BadRequest));

        Assert.True(result.IsFailure);
        Assert.DoesNotContain(marker, result.PrimaryError.Description, StringComparison.Ordinal);
        Assert.DoesNotContain(marker, result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ApprovedTransportPolicy_PropagatesCallerCancellation()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var exception = new TaskCanceledException("cancelled by caller");

        var thrown = Assert.Throws<OperationCanceledException>(() =>
            PackageHttpExtensions.MapTransportExceptionToResult<int>(exception, PackageContext, source.Token));

        Assert.Equal(source.Token, thrown.CancellationToken);
    }

    [Theory]
    [MemberData(nameof(TransportExceptions))]
    public void ApprovedTransportPolicy_MapsNonCallerTransportFailures(Exception exception)
    {
        var result = PackageHttpExtensions.MapTransportExceptionToResult<int>(
            exception,
            PackageContext,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<PackageHttpError>(result.PrimaryError);
        Assert.Equal(PackageHttpErrorType.TransportFailure, error.HttpErrorType);
        Assert.Same(exception, error.Exception);

        var apiResponse = PackageHttpMapper.ToHttpResponse(
            PackageResult.Failure(error, ExternalServiceResponsePolicy.TransportFailureMessage),
            CreateApprovedHttpPolicy());
        var body = Assert.IsType<LegacyErrorResponseBody>(apiResponse.Body);
        Assert.Equal(503, apiResponse.StatusCode);
        Assert.Equal(ExternalServiceResponsePolicy.TransportFailureMessage, body.Message);
        Assert.DoesNotContain(exception.Message, body.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ErrorLogContext_PreservesMediaVaultOwnedOriginFields()
    {
        var context = new ErrorLogContext("Infrastructure", "RawgApiClient", "SearchGamesAsync");

        Assert.Equal("Infrastructure", context.Layer);
        Assert.Equal("RawgApiClient", context.Service);
        Assert.Equal("SearchGamesAsync", context.Method);
    }

    public static IEnumerable<object[]> TransportExceptions()
    {
        yield return [new HttpRequestException("transport")];
        yield return [new TimeoutException("timeout")];
        yield return [new TaskCanceledException("non-caller timeout")];
    }

    private static PackageHttpPolicy CreateApprovedHttpPolicy() =>
        PackageHttpPolicy.Default with
        {
            ErrorTypeStatusCode = errorType =>
                errorType switch
                {
                    PackageErrorType.Validation => 422,
                    PackageErrorType.NotFound => 404,
                    PackageErrorType.Conflict => 409,
                    PackageErrorType.Unauthorized => 401,
                    PackageErrorType.Forbidden => 403,
                    PackageErrorType.Failure => 500,
                    PackageErrorType.Cancelled => 503,
                    PackageErrorType.External => 500,
                    _ => 400
                },
            FailureBodyFactory = CreateApprovedFailureBody
        };

    private static object CreateApprovedFailureBody(PackageResult result)
    {
        if (result.PrimaryError.Type == PackageErrorType.Validation)
        {
            var items = result.ValidationErrors
                .Select(x => new LegacyValidationErrorItem(x.FieldName, x.UserMessage))
                .ToArray();
            return new LegacyValidationErrorResponseBody(result.Message, items);
        }

        return new LegacyErrorResponseBody(result.Message, result.PrimaryError.Code);
    }

    private static PackageHttpResponseOptions CreateOutboundOptions(HttpStatusCode statusCode) =>
        new()
        {
            MaxResponseBodyBytes = ExternalServiceResponsePolicy.MaxInspectedBodyBytes,
            UserMessageFactory = _ => ExternalServiceResponsePolicy.GetSafeUserMessage(statusCode)
        };

    private static string CreatePayloadJson(int byteCount)
    {
        const string prefix = "{\"value\":\"";
        const string suffix = "\"}";
        var valueLength = byteCount - Encoding.UTF8.GetByteCount(prefix + suffix);
        var json = prefix + new string('x', valueLength) + suffix;

        Assert.Equal(byteCount, Encoding.UTF8.GetByteCount(json));
        return json;
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string content) =>
        new(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    private static HttpResponseMessage TextResponse(HttpStatusCode statusCode, string content) =>
        new(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "text/plain")
        };

    private static string SerializeContract(object body) =>
        JsonSerializer.Serialize(body, body.GetType(), new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private sealed record PayloadDto(string Value);
}
