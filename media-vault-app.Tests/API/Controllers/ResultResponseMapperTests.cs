using System.Text.Json;
using media_vault_app.API.Controllers;
using Megaraz.ResultPattern;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PackageDatabaseError = Megaraz.ResultPattern.Infrastructure.DatabaseError;
using PackageHttpError = Megaraz.ResultPattern.AspNetCore.HttpError;

namespace media_vault_app.Tests.API.Controllers;

public class ResultResponseMapperTests
{
    private static readonly ErrorContext Context = new(OperationType.Get, "MediaEntry");

    [Fact]
    public void ToActionResult_MapsSuccessfulTypedResultTo200WithValue()
    {
        var action = ResultResponseMapper.ToActionResult(new TestController(), Result<int>.Success(42));

        var result = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ToNoContentResult_MapsSuccessfulResultTo204()
    {
        var action = ResultResponseMapper.ToNoContentResult(new TestController(), Result.Success());

        Assert.IsType<NoContentResult>(action);
    }

    [Fact]
    public void ToNoContentResult_MapsDatabaseConcurrencyFailureToSafe409Body()
    {
        var error = PackageDatabaseError.ConcurrencyFailure(
            Context,
            new InvalidOperationException("private database detail"),
            "The resource changed after it was read.");

        var action = ResultResponseMapper.ToNoContentResult(
            new TestController(),
            Result.Failure(error));

        var result = Assert.IsType<ObjectResult>(action);
        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        var body = Assert.IsType<ErrorResponseBody>(result.Value);
        Assert.Equal(error.Code, body.Code);
        Assert.Equal("The resource changed after it was read.", body.Message);
        Assert.DoesNotContain("private database detail", body.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ToCreatedResult_PreservesCreatedAtActionRouteValuesBodyAndLocation()
    {
        var action = ResultResponseMapper.ToCreatedResult(
            new TestController(),
            Result<int>.Success(42),
            "GetById",
            value => new { id = value });

        var result = Assert.IsType<CreatedAtActionResult>(action.Result);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal("GetById", result.ActionName);
        Assert.Equal(42, result.RouteValues!["id"]);
        Assert.Equal(42, result.Value);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore();
        services.AddSingleton<IUrlHelperFactory>(new TestUrlHelperFactory());
        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        await result.ExecuteResultAsync(new ActionContext(httpContext, new RouteData(), new ActionDescriptor()));

        Assert.Equal("/MediaEntries/42", httpContext.Response.Headers.Location);
    }

    [Theory]
    [MemberData(nameof(FailureCases))]
    public void ToActionResult_MapsFailureToApprovedStatusAndSafeOrdinaryBody(
        Error error,
        int expectedStatus)
    {
        const string message = "Safe failure message.";
        var action = ResultResponseMapper.ToActionResult(
            new TestController(),
            Result<int>.Failure(error, message));

        var result = Assert.IsType<ObjectResult>(action.Result);
        Assert.Equal(expectedStatus, result.StatusCode);
        var body = Assert.IsType<ErrorResponseBody>(result.Value);
        Assert.Equal(message, body.Message);
        Assert.Equal(error.Code, body.Code);
    }

    [Fact]
    public void ToActionResult_MapsValidationTo422WithoutPublicCodes()
    {
        var validationError = ValidationError.Required(
            new ErrorContext(OperationType.Create, "MediaEntry", "Title"),
            userMessage: "Title is required.");
        var action = ResultResponseMapper.ToActionResult(
            new TestController(),
            Result<int>.ValidationFailure([validationError], "Validation failed."));

        var result = Assert.IsType<ObjectResult>(action.Result);
        Assert.Equal(422, result.StatusCode);
        var body = Assert.IsType<ValidationErrorResponseBody>(result.Value);
        var json = JsonSerializer.Serialize(body, body.GetType(), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(["message", "validationErrors"], root.EnumerateObject().Select(property => property.Name));
        Assert.False(root.TryGetProperty("code", out _));
        var item = Assert.Single(root.GetProperty("validationErrors").EnumerateArray());
        Assert.Equal(["field", "message"], item.EnumerateObject().Select(property => property.Name));
        Assert.False(item.TryGetProperty("code", out _));
    }

    public static IEnumerable<object[]> FailureCases()
    {
        yield return [PackageHttpError.BadRequest(Context), 400];
        yield return [Error.Unauthorized(Context), 401];
        yield return [Error.Custom("Get.MediaEntry.Forbidden", "Forbidden.", ErrorType.Forbidden, "Forbidden."), 403];
        yield return [Error.NotFound(Context), 404];
        yield return [Error.Conflict(Context), 409];
        yield return [PackageHttpError.UnprocessableContent(Context), 422];
        yield return [PackageHttpError.TooManyRequests(Context), 429];
        yield return [Error.Failure(Context), 500];
        yield return [PackageHttpError.InternalServerError(Context), 502];
        yield return [PackageHttpError.TransportFailure(Context, new TimeoutException("private timeout detail")), 503];
        yield return [Error.Cancelled(Context), 503];
        yield return [PackageDatabaseError.QueryFailure(Context, new Exception("private database detail")), 500];
        yield return [Error.Custom("Get.MediaEntry.External", "External.", ErrorType.External, "External failure."), 500];
    }

    private sealed class TestController : ControllerBase;

    private sealed class TestUrlHelperFactory : IUrlHelperFactory
    {
        public IUrlHelper GetUrlHelper(ActionContext context) => new TestUrlHelper(context);
    }

    private sealed class TestUrlHelper(ActionContext actionContext) : IUrlHelper
    {
        public ActionContext ActionContext { get; } = actionContext;

        public string? Action(UrlActionContext actionContext) => "/MediaEntries/42";

        public string? Content(string? contentPath) => contentPath;

        public bool IsLocalUrl(string? url) => true;

        public string? Link(string? routeName, object? values) => null;

        public string? RouteUrl(UrlRouteContext routeContext) => null;
    }
}
