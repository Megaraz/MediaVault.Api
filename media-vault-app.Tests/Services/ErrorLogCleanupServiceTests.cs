using media_vault_app.Application.Services;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Diagnostics;
using ErrorLog = Rasmus.SharedKernel.ResultPattern.ErrorLog;

namespace media_vault_app.Tests.Services
{
    public class ErrorLogCleanupServiceTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_Invoke_Cleanup_Before_Stopping()
        {
            using var cts = new CancellationTokenSource();
            var logger = new FakeErrorLogger(cts);
            var service = new TestableErrorLogCleanupService(logger);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RunAsync(cts.Token));

            Assert.Equal(1, logger.CleanOldLogsCallCount);
        }

        private sealed class TestableErrorLogCleanupService : ErrorLogCleanupService
        {
            public TestableErrorLogCleanupService(IErrorLogger logger)
                : base(logger)
            {
            }

            public Task RunAsync(CancellationToken cancellationToken)
            {
                return ExecuteAsync(cancellationToken);
            }
        }

        private sealed class FakeErrorLogger : IErrorLogger
        {
            private readonly CancellationTokenSource _cts;

            public FakeErrorLogger(CancellationTokenSource cts)
            {
                _cts = cts;
            }

            public int CleanOldLogsCallCount { get; private set; }

            public Task CleanOldLogsAsync(CancellationToken ct = default)
            {
                CleanOldLogsCallCount++;
                _cts.Cancel();
                return Task.CompletedTask;
            }

            public Task<IReadOnlyList<ErrorLog>> GetErrorLogsAsync(CancellationToken ct = default)
            {
                return Task.FromResult<IReadOnlyList<ErrorLog>>(Array.Empty<ErrorLog>());
            }

            public Task LogErrorToFileAsync(Error error, ErrorLogContext context, CancellationToken ct = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}
