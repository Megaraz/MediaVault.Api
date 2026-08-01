using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Diagnostics;
using ErrorLog = Rasmus.SharedKernel.ResultPattern.ErrorLog;

namespace Rasmus.SharedKernel.Interfaces.ErrorLogger
{
    public interface IErrorLogger
    {
        Task CleanOldLogsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ErrorLog>> GetErrorLogsAsync(CancellationToken ct = default);
        Task LogErrorToFileAsync(Error error, ErrorLogContext context, CancellationToken ct = default);
    }
}
