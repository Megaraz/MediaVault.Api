using Megaraz.ResultPattern;
using Microsoft.Extensions.Logging;

namespace media_vault_app.Application.Services
{
    internal static partial class ServiceValidationLogging
    {
        private const int MaxLoggedErrorCodes = 10;

        internal static void LogValidationFailure(
            ILogger logger,
            IEnumerable<ValidationError> validationErrors,
            string service,
            string method,
            ErrorContext context)
        {
            var errorCodes = string.Join(",", validationErrors
                .Select(error => error.Code)
                .Take(MaxLoggedErrorCodes));

            ApplicationValidationFailed(
                logger,
                "Application",
                service,
                method,
                context.Operation.ToString(),
                context.EntityName,
                errorCodes);
        }

        [LoggerMessage(
            EventId = 1000,
            EventName = "ApplicationValidationFailed",
            Level = LogLevel.Debug,
            Message = "Application validation failed in {Layer}.{Service}.{Method} for {Operation} {EntityName}: {ErrorCodes}")]
        private static partial void ApplicationValidationFailed(
            ILogger logger,
            string layer,
            string service,
            string method,
            string operation,
            string entityName,
            string errorCodes);
    }
}
