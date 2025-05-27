using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public interface ILoggingService
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(Exception ex, string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void LogTrace(string message, params object[] args);
        IDisposable BeginScope<TState>(TState state);
        Task LogWithContextAsync(string message, LogLevel level, IDictionary<string, object> properties);
    }

    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public LoggingService(ILogger<LoggingService> logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        public void LogError(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
        }

        public void LogTrace(string message, params object[] args)
        {
            _logger.LogTrace(message, args);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }

        public async Task LogWithContextAsync(string message, LogLevel level, IDictionary<string, object> properties)
        {
            using (LogContext.PushProperties(properties))
            {
                switch (level)
                {
                    case LogLevel.Information:
                        LogInformation(message);
                        break;
                    case LogLevel.Warning:
                        LogWarning(message);
                        break;
                    case LogLevel.Error:
                        LogError(new Exception(message), message);
                        break;
                    case LogLevel.Debug:
                        LogDebug(message);
                        break;
                    case LogLevel.Trace:
                        LogTrace(message);
                        break;
                }
            }
            await Task.CompletedTask;
        }
    }
}