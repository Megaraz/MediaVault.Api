using Megaraz.ResultPattern;
using Megaraz.ResultPattern.AspNetCore;
using Megaraz.ResultPattern.Infrastructure;

namespace media_vault_app.Infrastructure.Diagnostics;

public enum ErrorEventKind
{
    DatabaseConcurrencyConflict,
    DatabaseOperationFailed,
    ExternalDependencyTransientFailure,
    ExternalDependencyAuthenticationFailed,
    ExternalDependencyInvalidResponse
}

/// <summary>
/// Classifies known Result failures into the approved MediaVault operational events.
/// </summary>
public sealed class ErrorEventPolicy
{
    public ErrorEventKind? GetEventKind(Error error) => error switch
    {
        _ when error.Type == ErrorType.Cancelled => null,
        ValidationError => null,
        DatabaseError when error.Code.EndsWith(".DatabaseConcurrencyFailure", StringComparison.Ordinal) =>
            ErrorEventKind.DatabaseConcurrencyConflict,
        DatabaseError => ErrorEventKind.DatabaseOperationFailed,
        HttpError httpError => GetHttpEventKind(httpError.HttpErrorType),
        _ => null
    };

    private static ErrorEventKind? GetHttpEventKind(HttpErrorType errorType) => errorType switch
    {
        HttpErrorType.BadRequest or
        HttpErrorType.NotFound or
        HttpErrorType.Conflict or
        HttpErrorType.UnprocessableContent => null,
        HttpErrorType.Unauthorized or HttpErrorType.Forbidden =>
            ErrorEventKind.ExternalDependencyAuthenticationFailed,
        HttpErrorType.InternalServerError or
        HttpErrorType.TooManyRequests or
        HttpErrorType.TransportFailure => ErrorEventKind.ExternalDependencyTransientFailure,
        HttpErrorType.Custom or
        HttpErrorType.MalformedResponse or
        HttpErrorType.UnexpectedStatusCode => ErrorEventKind.ExternalDependencyInvalidResponse,
        _ => null
    };
}
