using System.Runtime.CompilerServices;
using media_vault_app.Infrastructure.Diagnostics;
using Megaraz.ResultPattern;
using Megaraz.ResultPattern.AspNetCore;
using Polly.Timeout;
using Rasmus.SharedKernel.ExternalServices;
using PackageHttpExtensions = Megaraz.ResultPattern.AspNetCore.HttpResponseToResultExtensions;

namespace media_vault_app.Infrastructure.API.Clients
{
    public abstract class ApiClientBase<TCategory>
        where TCategory : class
    {
        // Package defaults preserve web JSON and missing-content-type compatibility while extracting
        // bounded technical diagnostics. Public messages are replaced after mapping because the
        // package callback does not receive the response status needed by MediaVault's fixed policy.
        private static readonly HttpResponseMappingOptions ResponseMappingOptions = new()
        {
            MaxResponseBodyBytes = ExternalServiceResponsePolicy.MaxInspectedBodyBytes
        };

        private readonly ErrorEventLogger<TCategory> _errorEventLogger;
        private readonly string _provider;

        protected ApiClientBase(
            ErrorEventLogger<TCategory> errorEventLogger,
            string provider)
        {
            _errorEventLogger = errorEventLogger;
            _provider = provider;
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

                LogIfNeeded(result.PrimaryError, errorContext, methodName, (int)response.StatusCode);
                return result;
            }
            catch (Exception exception) when (
                exception is HttpRequestException or TimeoutException or TaskCanceledException or TimeoutRejectedException)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException(cancellationToken);

                var mappedResult = exception is TimeoutRejectedException
                    ? Result<TValue>.Failure(HttpError.TransportFailure(errorContext, exception))
                    : PackageHttpExtensions.MapTransportExceptionToResult<TValue>(
                        exception,
                        errorContext,
                        cancellationToken);
                var result = Result<TValue>.Failure(
                    mappedResult.PrimaryError,
                    ExternalServiceResponsePolicy.TransportFailureMessage);

                LogIfNeeded(result.PrimaryError, errorContext, methodName);
                return result;
            }
        }

        private void LogIfNeeded(
            Error? error,
            ErrorContext errorContext,
            string methodName,
            int? statusCode = null)
        {
            if (error is null || error.Type == ErrorType.None)
                return;

            var failureKind = error is HttpError httpError
                ? httpError.HttpErrorType.ToString()
                : error.Type.ToString();
            var context = new ErrorEventContext(
                "Infrastructure",
                GetType().Name,
                methodName,
                errorContext,
                _provider,
                failureKind,
                statusCode);
            _errorEventLogger.Log(error, context);
        }
    }
}
