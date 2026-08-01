using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Diagnostics;
using Rasmus.SharedKernel.ResultPatternCompatibility;
using LegacyErrorLogger = Rasmus.SharedKernel.ResultPattern.ErrorLogger;
using LegacyErrorLoggerConfiguration = Rasmus.SharedKernel.ResultPattern.ErrorLoggerConfiguration;
using LegacyErrorLogPolicy = Rasmus.SharedKernel.ResultPattern.ErrorLogPolicy;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class ErrorLogger_Tests
    {

        //private ErrorLogger _errorLogger = new(
        //    new LegacyErrorLoggerConfiguration());


        [Fact]
        public async Task LogErrorToFileAsync_ShouldLogErrorToFile()
        {
            var tempDir = CreateTempDirectory();
            try
            {
                var errorLogger = new LegacyErrorLogger(new LegacyErrorLoggerConfiguration { BasePath = tempDir });
                var error = Error.Failure(
                    PackageErrorContextFactory.Create(),
                    "Test error description",
                    new Exception("Test exception"));

                await errorLogger.LogErrorToFileAsync(error, CreateLogContext());

                Assert.NotEmpty(await errorLogger.GetErrorLogsAsync());
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public async Task LogErrorToFileAsync_Writes_The_Characterized_Ndjson_Schema()
        {
            var tempDir = CreateTempDirectory();
            try
            {
                var configuration = new LegacyErrorLoggerConfiguration
                {
                    BasePath = tempDir,
                    Filename = "schema.log.ndjson"
                };
                var errorLogger = new LegacyErrorLogger(configuration);
                var error = Error.Failure(
                    PackageErrorContextFactory.Create(),
                    "Technical description",
                    new InvalidOperationException("diagnostic exception"));

                await errorLogger.LogErrorToFileAsync(error, CreateLogContext());

                var lines = await File.ReadAllLinesAsync(configuration.FullPath);
                var line = Assert.Single(lines);
                using var document = System.Text.Json.JsonDocument.Parse(line);
                var root = document.RootElement;

                Assert.Equal(
                    ["writeDate", "code", "description", "errorType", "layer", "service", "method", "exceptionMessage", "stackTrace"],
                    root.EnumerateObject().Select(x => x.Name));
                Assert.Equal(error.Code, root.GetProperty("code").GetString());
                Assert.Equal(error.Description, root.GetProperty("description").GetString());
                Assert.Equal(error.Type.ToString(), root.GetProperty("errorType").GetString());
                Assert.Equal("Infrastructure", root.GetProperty("layer").GetString());
                Assert.Equal("ErrorLoggerTests", root.GetProperty("service").GetString());
                Assert.Equal("TestMethod", root.GetProperty("method").GetString());
                Assert.Equal("diagnostic exception", root.GetProperty("exceptionMessage").GetString());
                Assert.Equal(System.Text.Json.JsonValueKind.Null, root.GetProperty("stackTrace").ValueKind);
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void ErrorLoggerConfiguration_Defaults_ArePartOfTheOperationalContract()
        {
            var configuration = new LegacyErrorLoggerConfiguration();

            Assert.Equal(TimeSpan.FromDays(7), configuration.RetentionPeriod);
            Assert.Equal("errors.log.ndjson", configuration.Filename);
        }

        [Fact]
        public async Task CleanOldLogsAsync_CorruptedEntry_IsDroppedWithoutThrowing()
        {
            // Arrange
            var tempDir = CreateTempDirectory();
            try
            {
                var config = new LegacyErrorLoggerConfiguration
                {
                    BasePath = tempDir,
                    Filename = "test-errors.log.ndjson",
                    RetentionPeriod = TimeSpan.FromDays(7),
                };
                LegacyErrorLogger errorLogger = new(config);

                // Write one valid entry and one corrupted entry directly
                Error validError = Error.Failure(PackageErrorContextFactory.Create(), "Valid entry");
                await errorLogger.LogErrorToFileAsync(validError, CreateLogContext());
                await File.AppendAllTextAsync(config.FullPath, "{ this is not valid json }" + Environment.NewLine);

                // Act — must not throw
                await errorLogger.CleanOldLogsAsync();

                // Assert — valid entry survives; corrupted entry is silently dropped
                var logs = await errorLogger.GetErrorLogsAsync();
                Assert.Single(logs);
                Assert.Contains("Valid entry", logs[0].Description);
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public async Task GetErrorLogsAsync_CorruptedEntry_IsDroppedWithoutThrowing()
        {
            // Arrange
            var tempDir = CreateTempDirectory();
            try
            {
                var config = new LegacyErrorLoggerConfiguration
                {
                    BasePath = tempDir,
                    Filename = "test-errors.log.ndjson",
                    RetentionPeriod = TimeSpan.FromDays(7),
                };
                LegacyErrorLogger errorLogger = new(config);

                Error validError = Error.Failure(PackageErrorContextFactory.Create(), "Valid entry");
                await errorLogger.LogErrorToFileAsync(validError, CreateLogContext());
                await File.AppendAllTextAsync(config.FullPath, "{ this is not valid json }" + Environment.NewLine);

                // Act — must not throw
                var logs = await errorLogger.GetErrorLogsAsync();

                // Assert — valid entry returned; corrupted entry silently dropped
                Assert.Single(logs);
                Assert.Contains("Valid entry", logs[0].Description);
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public async Task LogErrorToFileAsync_ConcurrentWrites_ShouldNotLoseEntries()
        {
            // Arrange — unique file per test run to avoid cross-test interference
            var tempDir = CreateTempDirectory();
            try
            {
                var config = new LegacyErrorLoggerConfiguration
                {
                    BasePath = tempDir,
                    Filename = "concurrent-test.log.ndjson",
                };
                var errorLogger = new LegacyErrorLogger(config);

                const int concurrentWriters = 20;

                // Act — fire all writes simultaneously
                var tasks = Enumerable.Range(0, concurrentWriters)
                    .Select(i => errorLogger.LogErrorToFileAsync(
                        Error.Failure(PackageErrorContextFactory.Create(), $"Concurrent entry {i}"),
                        CreateLogContext()))
                    .ToList();

                await Task.WhenAll(tasks);

                // Assert — every write must have landed; nothing lost to a race
                var logs = await errorLogger.GetErrorLogsAsync();
                Assert.Equal(concurrentWriters, logs.Count);
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        private static string CreateTempDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(path);
            return path;
        }

        private static ErrorLogContext CreateLogContext() =>
            new("Infrastructure", "ErrorLoggerTests", "TestMethod");

    }
}
