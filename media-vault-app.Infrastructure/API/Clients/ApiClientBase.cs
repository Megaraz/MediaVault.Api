using System.Runtime.CompilerServices;
using Megaraz.ResultPattern;
using Megaraz.ResultPattern.AspNetCore;
using Rasmus.SharedKernel.Diagnostics;
using Rasmus.SharedKernel.ExternalServices;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;
using PackageHttpExtensions = Megaraz.ResultPattern.AspNetCore.HttpResponseToResultExtensions;

namespace media_vault_app.Infrastructure.API.Clients
{
    public abstract class ApiClientBase
    {
        // Package defaults preserve web JSON and missing-content-type compatibility while extracting
        // bounded technical diagnostics. Public messages are replaced after mapping because the
        // package callback does not receive the response status needed by MediaVault's fixed policy.
        private static readonly HttpResponseMappingOptions ResponseMappingOptions = new()
        {
            MaxResponseBodyBytes = ExternalServiceResponsePolicy.MaxInspectedBodyBytes
        };

        private readonly IErrorLogger _errorLogger;
        private readonly IErrorLogPolicy _errorLogPolicy;

        protected ApiClientBase(IErrorLogger errorLogger, IErrorLogPolicy errorLogPolicy)
        {
            _errorLogger = errorLogger;
            _errorLogPolicy = errorLogPolicy;
        }

        protected async Task<Result<TValue>> SendAndMapAsync<TValue>(
            Func<CancellationToken, Task<HttpResponseMessage>> sendAsync,
            ErrorContext errorContext,
            CancellationToken cancellationToken,
            [CallerMemberName] string methodName = "")
            where TValue : notnull
        {
            try
            {
                using var response = await sendAsync(cancellationToken);
                var result = await PackageHttpExtensions.MapToResultAsync<TValue>(
                    response,
                    errorContext,
                    ResponseMappingOptions,
                    cancellationToken);

                if (result.IsFailure)
                {
                    result = Result<TValue>.Failure(
                        result.PrimaryError,
                        ExternalServiceResponsePolicy.GetSafeUserMessage(response.StatusCode));
                }

                await LogIfNeededAsync(result.PrimaryError, methodName);
                return result;
            }
            catch (Exception exception) when (
                exception is HttpRequestException or TimeoutException or TaskCanceledException)
            {
                var mappedResult = PackageHttpExtensions.MapTransportExceptionToResult<TValue>(
                    exception,
                    errorContext,
                    cancellationToken);
                var result = Result<TValue>.Failure(
                    mappedResult.PrimaryError,
                    ExternalServiceResponsePolicy.TransportFailureMessage);

                await LogIfNeededAsync(result.PrimaryError, methodName);
                return result;
            }
        }

        protected async Task LogIfNeededAsync(
            Error? error,
            string methodName)
        {
            if (error is null || error.Type == ErrorType.None || !ShouldLog(error))
                return;

            try
            {
                var context = new ErrorLogContext("Infrastructure", GetType().Name, methodName);
                await _errorLogger.LogErrorToFileAsync(error, context, CancellationToken.None);
            }
            catch
            {
                // Logging must not break the API client result flow.
            }
        }

        private bool ShouldLog(Error error)
        {
            if (error is not HttpError httpError)
                return _errorLogPolicy.ShouldLog(error);

            return httpError.HttpErrorType switch
            {
                HttpErrorType.BadRequest => false,
                HttpErrorType.NotFound => false,
                HttpErrorType.Conflict => false,
                HttpErrorType.UnprocessableContent => false,
                _ => true
            };
        }
    }
}
