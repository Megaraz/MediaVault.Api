using Rasmus.SharedKernel.Interfaces.ErrorLogger;
using Rasmus.SharedKernel.ResultPattern;

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

        protected async Task LogIfNeededAsync(Error? error, CancellationToken ct)
        {
            if (error is null || error.Type == ErrorType.None || !_errorLogPolicy.ShouldLog(error))
                return;

            try
            {
                await _errorLogger.LogErrorToFileAsync(error, CancellationToken.None);
            }
            catch
            {
                // Logging must not break the API client result flow.
            }
        }
    }
}
