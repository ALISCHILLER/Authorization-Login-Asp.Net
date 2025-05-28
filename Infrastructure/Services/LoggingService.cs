using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Authorization_Login_Asp.Net.Application.Interfaces;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public LoggingService(ILogger<LoggingService> logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public async Task LogInformationAsync(string message)
        {
            _logger.LogInformation(message);
            await Task.CompletedTask;
        }

        public async Task LogErrorAsync(Exception exception, string message)
        {
            _logger.LogError(exception, message);
            await Task.CompletedTask;
        }

        public async Task LogWarningAsync(string message)
        {
            _logger.LogWarning(message);
            await Task.CompletedTask;
        }

        public async Task LogCriticalAsync(Exception exception, string message)
        {
            _logger.LogCritical(exception, message);
            await Task.CompletedTask;
        }

        public async Task LogDebugAsync(string message)
        {
            _logger.LogDebug(message);
            await Task.CompletedTask;
        }

        public async Task LogTraceAsync(string message, params object[] args)
        {
            _logger.LogTrace(message, args);
            await Task.CompletedTask;
        }
    }
}