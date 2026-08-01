using System.Net;
using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.ResultPatternCompatibility;

/// <summary>
/// Temporary adapter that keeps the legacy HTTP classification until issue #92 adopts
/// Megaraz.ResultPattern.AspNetCore. It deliberately uses package core Error types.
/// </summary>
public enum HttpErrorType
{
    Custom = 0,
    BadRequest = 1,
    Unauthorized = 2,
    Forbidden = 3,
    NotFound = 4,
    Conflict = 5,
    InternalServerError = 6,
    UnprocessableContent = 7,
    TooManyRequests = 8,
    TransportFailure = 9,
    MalformedResponse = 10,
    UnexpectedStatusCode = 11
}

public record HttpError : Megaraz.ResultPattern.Error
{
    public HttpErrorType HttpErrorType { get; }

    private HttpError(
        string code,
        string description,
        HttpErrorType type,
        string userMessage,
        Exception? exception = null)
        : base(code, description, Megaraz.ResultPattern.ErrorType.External, userMessage, exception)
    {
        HttpErrorType = type;
    }

    public static HttpError Custom(ErrorContext context, string description) =>
        Create(context, "Custom", HttpErrorType.Custom, description, description);

    public static HttpError TransportFailure(ErrorContext context, Exception? exception = null) =>
        Create(context, "TransportFailure", HttpErrorType.TransportFailure, "Transport Failure", "Transport Failure", exception);

    public static HttpError TooManyRequests(ErrorContext context, string? callerMessage = null) =>
        CreateWithCallerMessage(context, "TooManyRequests", HttpErrorType.TooManyRequests, callerMessage, "Too Many Requests");

    public static HttpError MalformedResponse(ErrorContext context, Exception? exception = null, string? detail = null)
    {
        const string userMessage = "The external service returned a malformed or unexpected response.";
        return Create(context, "MalformedResponse", HttpErrorType.MalformedResponse,
            string.IsNullOrWhiteSpace(detail) ? userMessage : detail, userMessage, exception);
    }

    public static HttpError UnexpectedStatusCode(ErrorContext context, HttpStatusCode statusCode)
    {
        var message = $"The external service returned an unexpected HTTP status code {(int)statusCode} ({statusCode}).";
        return Create(context, "UnexpectedStatusCode", HttpErrorType.UnexpectedStatusCode, message, message);
    }

    public static HttpError UnprocessableContent(ErrorContext context, string? callerMessage = null) =>
        CreateWithCallerMessage(context, "UnprocessableContent", HttpErrorType.UnprocessableContent, callerMessage, "Unprocessable Content");

    public static HttpError BadRequest(ErrorContext context, string? callerMessage = null) =>
        CreateWithCallerMessage(context, "BadRequest", HttpErrorType.BadRequest, callerMessage, "Bad Request");

    public static HttpError UnauthorizedAccess(ErrorContext context, string? callerMessage = null) =>
        CreateWithCallerMessage(context, "Unauthorized", HttpErrorType.Unauthorized, callerMessage, "Unauthorized");

    public static HttpError Forbidden(ErrorContext context, string? callerMessage = null) =>
        CreateWithCallerMessage(context, "Forbidden", HttpErrorType.Forbidden, callerMessage, "Forbidden");

    public static HttpError NotFound(ErrorContext context, string? callerMessage = null) =>
        CreateWithCallerMessage(context, "NotFound", HttpErrorType.NotFound, callerMessage, "Not Found");

    public static HttpError Conflict(ErrorContext context, string? callerMessage = null) =>
        CreateWithCallerMessage(context, "Conflict", HttpErrorType.Conflict, callerMessage, "Conflict");

    public static HttpError InternalServerError(ErrorContext context, string? callerMessage = null) =>
        CreateWithCallerMessage(context, "InternalServerError", HttpErrorType.InternalServerError, callerMessage, "Internal Server Error");

    private static HttpError CreateWithCallerMessage(
        ErrorContext context,
        string reason,
        HttpErrorType type,
        string? callerMessage,
        string defaultMessage)
    {
        var message = string.IsNullOrWhiteSpace(callerMessage) ? defaultMessage : callerMessage;
        return Create(context, reason, type, message, message);
    }

    private static HttpError Create(
        ErrorContext context,
        string reason,
        HttpErrorType type,
        string descriptionDetail,
        string userMessage,
        Exception? exception = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        var code = Megaraz.ResultPattern.ErrorCode.For(context, reason).Code;
        var description = TemporaryResultPatternBridge.FormatDescription(context, descriptionDetail);
        return new HttpError(code, description, type, userMessage, exception);
    }
}
