using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Hosting;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;

namespace media_vault_app.Application.Services
{
    public class ErrorLogCleanupService : BackgroundService
    {
        private readonly IErrorLogger _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(24);

        public ErrorLogCleanupService(IErrorLogger logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _logger.CleanOldLogsAsync(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
