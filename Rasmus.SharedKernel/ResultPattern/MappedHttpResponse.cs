namespace Rasmus.SharedKernel.ResultPattern
{
    /// <summary>
    /// Framework-agnostic HTTP response descriptor produced by mapping a domain <see cref="Result"/>.
    /// </summary>
    public sealed record MappedHttpResponse(int StatusCode, object? Body = null, string? Location = null);

    /// <summary>
    /// Error response body containing a message and error code.
    /// </summary>
    public sealed record ErrorResponseBody(string Message, string Code);

    /// <summary>
    /// Validation error response body containing a message and a list of validation error codes.
    /// </summary>
    public sealed record ValidationErrorResponseBody(string Message, IEnumerable<string>? ValidationErrors);
}
