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

    }
}
