using System.Diagnostics;
using System.Text.Json;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;

namespace Rasmus.SharedKernel.ResultPattern
{

    public sealed class ErrorLoggerConfiguration
    {
        public TimeSpan RetentionPeriod { get; init; } = TimeSpan.FromDays(7);
        public string BasePath { get; init; } = AppDomain.CurrentDomain.BaseDirectory;
        public string Filename { get; init; } = "errors.log.ndjson";
        public string FullPath => Path.Combine(BasePath, Filename);
    }

    public record ErrorLog
    (
        DateTimeOffset WriteDate,
        string Code,
        string Description,
        string ErrorType,
        string? ExceptionMessage,
        string? StackTrace
    );

    public class ErrorLogger : IErrorLogger
    {
        // One lock shared across all instances so that two loggers pointing at the same
        // file path cannot race each other. SemaphoreSlim(1,1) = mutex semantics.
        // "Slim" is used instead of lock{} because await requires an async-compatible wait.
        private static readonly SemaphoreSlim s_fileLock = new SemaphoreSlim(1, 1);

        private readonly ErrorLoggerConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public ErrorLogger(ErrorLoggerConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task CleanOldLogsAsync(CancellationToken ct = default)
        {
            if (!File.Exists(_configuration.FullPath))
                return;

            // Acquire the lock before touching the file.
            // WaitAsync forwards the CancellationToken: if cancelled before we acquire,
            // an OperationCanceledException is thrown and Release() is never called — which
            // is correct, because we never entered the protected section.
            await s_fileLock.WaitAsync(ct);
            try
            {
                var cutoffDate = DateTimeOffset.UtcNow - _configuration.RetentionPeriod;

                var lines = await File.ReadAllLinesAsync(_configuration.FullPath, ct);

                var recentLogs = lines
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(TryDeserializeLog)
                    .Where(log => log is not null && log.WriteDate >= cutoffDate)
                    .Cast<ErrorLog>()
                    .ToList();

                var newLines = recentLogs
                    .Select(log => JsonSerializer.Serialize(log, _jsonOptions));

                await File.WriteAllLinesAsync(_configuration.FullPath, newLines, ct);
            }
            finally
            {
                // Always release, even if an exception was thrown inside the try block.
                // Without this, any exception would permanently block all future callers.
                s_fileLock.Release();
            }
        }

        public async Task<IReadOnlyList<ErrorLog>> GetErrorLogsAsync(CancellationToken ct = default)
        {
            if (!File.Exists(_configuration.FullPath))
                return new List<ErrorLog>();

            // Reading also needs the lock: AppendAllTextAsync writes in chunks, so a
            // concurrent read could see a partial JSON line mid-flush.
            await s_fileLock.WaitAsync(ct);
            try
            {
                var lines = await File.ReadAllLinesAsync(_configuration.FullPath, ct);
                var logs = lines
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(TryDeserializeLog)
                    .Where(log => log is not null)
                    .Cast<ErrorLog>()
                    .ToList();
                return logs;
            }
            finally
            {
                s_fileLock.Release();
            }
        }

        public async Task LogErrorToFileAsync(Error error, CancellationToken ct = default)
        {
            Directory.CreateDirectory(_configuration.BasePath);

            var currentLogEntry = new ErrorLog(
                DateTimeOffset.UtcNow,
                error.Code,
                error.Description,
                error.Type.ToString(),
                error.Exception?.Message,
                error.Exception?.StackTrace);

            var json = JsonSerializer.Serialize(currentLogEntry, _jsonOptions);

            await s_fileLock.WaitAsync(ct);
            try
            {
                await File.AppendAllTextAsync(_configuration.FullPath, json + Environment.NewLine, ct);
            }
            finally
            {
                s_fileLock.Release();
            }
        }
        private ErrorLog? TryDeserializeLog(string line)
        {
            try
            {
                return JsonSerializer.Deserialize<ErrorLog>(line, _jsonOptions);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"[ErrorLogger] Skipping corrupted log entry: {ex.Message} | Line: {line}");
                return null;
            }
        }


    }
}
