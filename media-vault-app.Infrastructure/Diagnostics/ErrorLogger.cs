using System.Diagnostics;
using System.Text.Json;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Diagnostics;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;

namespace media_vault_app.Infrastructure.Diagnostics;

public sealed class ErrorLoggerConfiguration
{
    public TimeSpan RetentionPeriod { get; init; } = TimeSpan.FromDays(7);
    public string BasePath { get; init; } = AppDomain.CurrentDomain.BaseDirectory;
    public string Filename { get; init; } = "errors.log.ndjson";
    public string FullPath => Path.Combine(BasePath, Filename);
}

public sealed class ErrorLogger : IErrorLogger
{
    private static readonly SemaphoreSlim s_fileLock = new(1, 1);
    private readonly ErrorLoggerConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ErrorLogger(ErrorLoggerConfiguration configuration) => _configuration = configuration;

    public async Task CleanOldLogsAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_configuration.FullPath)) return;
        await s_fileLock.WaitAsync(ct);
        try
        {
            var cutoffDate = DateTimeOffset.UtcNow - _configuration.RetentionPeriod;
            var recentLogs = (await File.ReadAllLinesAsync(_configuration.FullPath, ct))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(TryDeserializeLog)
                .Where(log => log is not null && log.WriteDate >= cutoffDate)
                .Cast<ErrorLog>();
            await File.WriteAllLinesAsync(_configuration.FullPath, recentLogs.Select(log => JsonSerializer.Serialize(log, _jsonOptions)), ct);
        }
        finally { s_fileLock.Release(); }
    }

    public async Task<IReadOnlyList<ErrorLog>> GetErrorLogsAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_configuration.FullPath)) return Array.Empty<ErrorLog>();
        await s_fileLock.WaitAsync(ct);
        try
        {
            return (await File.ReadAllLinesAsync(_configuration.FullPath, ct))
                .Where(line => !string.IsNullOrWhiteSpace(line)).Select(TryDeserializeLog)
                .Where(log => log is not null).Cast<ErrorLog>().ToArray();
        }
        finally { s_fileLock.Release(); }
    }

    public async Task LogErrorToFileAsync(Error error, ErrorLogContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(context);
        Directory.CreateDirectory(_configuration.BasePath);
        var entry = new ErrorLog(DateTimeOffset.UtcNow, error.Code, error.Description, error.Type.ToString(), context.Layer, context.Service, context.Method, error.Exception?.Message, error.Exception?.StackTrace);
        await s_fileLock.WaitAsync(ct);
        try { await File.AppendAllTextAsync(_configuration.FullPath, JsonSerializer.Serialize(entry, _jsonOptions) + Environment.NewLine, ct); }
        finally { s_fileLock.Release(); }
    }

    private ErrorLog? TryDeserializeLog(string line)
    {
        try { return JsonSerializer.Deserialize<ErrorLog>(line, _jsonOptions); }
        catch (JsonException exception)
        {
            Debug.WriteLine($"[ErrorLogger] Skipping corrupted log entry: {exception.Message}");
            return null;
        }
    }
}
