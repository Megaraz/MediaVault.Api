using System;
using System.Collections.Generic;
using System.Text;
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

        public async Task<List<ErrorLog>> GetErrorLogsAsync(CancellationToken ct = default)
        {
            if (!File.Exists(_configuration.FullPath))
                return new List<ErrorLog>();

            var lines = await File.ReadAllLinesAsync(_configuration.FullPath, ct);
            var logs = lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<ErrorLog>(line, _jsonOptions))
                .Where(log => log is not null)
                .Cast<ErrorLog>()
                .ToList();
            return logs;
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

            await File.AppendAllTextAsync(_configuration.FullPath, json + Environment.NewLine, ct);

        }
        private ErrorLog? TryDeserializeLog(string line)
        {
            try
            {
                return JsonSerializer.Deserialize<ErrorLog>(line, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }


    }
}
