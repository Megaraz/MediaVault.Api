using System.Runtime.CompilerServices;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;
using Rasmus.SharedKernel.Diagnostics;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.ResultPatternCompatibility;

namespace media_vault_app.Infrastructure.API.Clients
{
    public abstract class ApiClientBase
    {
        private readonly IErrorLogger _errorLogger;
        private readonly IErrorLogPolicy _errorLogPolicy;

        protected ApiClientBase(IErrorLogger errorLogger, IErrorLogPolicy errorLogPolicy)
        {
            _errorLogger = errorLogger;
            _errorLogPolicy = errorLogPolicy;
        }

        protected async Task LogIfNeededAsync(
            Error? error,
            CancellationToken ct,
            [CallerMemberName] string methodName = "")
        {
            if (error is null || error.Type == ErrorType.None || !_errorLogPolicy.ShouldLog(error))
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
    }
}
