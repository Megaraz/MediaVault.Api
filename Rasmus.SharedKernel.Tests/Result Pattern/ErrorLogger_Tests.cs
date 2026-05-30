using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class ErrorLogger_Tests
    {

        //private ErrorLogger _errorLogger = new(
        //    new ErrorLoggerConfiguration());


        [Fact]
        public async Task LogErrorToFileAsync_ShouldLogErrorToFile()
        {
            // Arrange
            ErrorLogger errorLogger = new(
                new ErrorLoggerConfiguration());

            Error error = Error.Failure(TestErrorContextFactory.Create(), "Test error description", new Exception("Test exception"));

            // Act
            await errorLogger.LogErrorToFileAsync(error);

            // Assert
            Assert.NotEmpty(await errorLogger.GetErrorLogsAsync());


        }

        [Fact]
        public async Task CleanOldLogsAsync_CorruptedEntry_IsDroppedWithoutThrowing()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            try
            {
                var config = new ErrorLoggerConfiguration
                {
                    BasePath = tempDir,
                    Filename = "test-errors.log.ndjson",
                    RetentionPeriod = TimeSpan.FromDays(7),
                };
                ErrorLogger errorLogger = new(config);

                // Write one valid entry and one corrupted entry directly
                Error validError = Error.Failure(TestErrorContextFactory.Create(), "Valid entry");
                await errorLogger.LogErrorToFileAsync(validError);
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
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            try
            {
                var config = new ErrorLoggerConfiguration
                {
                    BasePath = tempDir,
                    Filename = "test-errors.log.ndjson",
                    RetentionPeriod = TimeSpan.FromDays(7),
                };
                ErrorLogger errorLogger = new(config);

                Error validError = Error.Failure(TestErrorContextFactory.Create(), "Valid entry");
                await errorLogger.LogErrorToFileAsync(validError);
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

    }
}
