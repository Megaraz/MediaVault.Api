using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace media_vault_app.Tests.TestHelpers
{
    internal static class ServiceTestLogger
    {
        public static ILogger<T> Create<T>() where T : class
        {
            return NullLogger<T>.Instance;
        }
    }
}