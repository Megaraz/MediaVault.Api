using Microsoft.Extensions.Logging;

namespace media_vault_app.Tests.TestHelpers;

internal sealed class RecordingLoggerProvider : ILoggerProvider
{
    public List<LogEntry> Entries { get; } = [];

    public ILogger CreateLogger(string categoryName) => new RecordingLogger(categoryName, Entries);

    public void Dispose()
    {
    }

    private sealed class RecordingLogger(string categoryName, List<LogEntry> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NoopScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var properties = state is IEnumerable<KeyValuePair<string, object?>> values
                ? values.ToDictionary(pair => pair.Key, pair => pair.Value)
                : new Dictionary<string, object?>();

            entries.Add(new LogEntry(
                categoryName,
                logLevel,
                eventId,
                properties,
                exception,
                formatter(state, exception)));
        }
    }

    private sealed class NoopScope : IDisposable
    {
        public static NoopScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}

internal sealed record LogEntry(
    string Category,
    LogLevel Level,
    EventId EventId,
    IReadOnlyDictionary<string, object?> Properties,
    Exception? Exception,
    string Message);
