using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services
{
    internal static class ServiceValidationLogging
    {
        internal static string FormatValidationErrors(IEnumerable<ValidationError> validationErrors)
        {
            return string.Join(
                Environment.NewLine,
                validationErrors.Select(error => $"{error.Code} - {error.Description}"));
        }
    }
}