using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Authorization_Login_Asp.Net.Infrastructure.Logging
{
    /// <summary>
    /// کلاس لاگر امنیتی برای ثبت رویدادهای امنیتی
    /// </summary>
    public class SecurityLogger : ISecurityLogger
    {
        private readonly ILogger<SecurityLogger> _logger;
        private readonly Serilog.ILogger _securityLogger;

        public SecurityLogger(ILogger<SecurityLogger> logger)
        {
            _logger = logger;
            _securityLogger = new LoggerConfiguration()
                .WriteTo.File("logs/security-.log", 
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();
        }

        public void LogFailedLoginAttempt(string username, string ipAddress, string reason)
        {
            _securityLogger.Warning(
                "Failed login attempt for user {Username} from IP {IpAddress}. Reason: {Reason}",
                username, ipAddress, reason);
        }

        public void LogPasswordChange(Guid userId, string ipAddress)
        {
            _securityLogger.Information(
                "Password changed for user {UserId} from IP {IpAddress}",
                userId, ipAddress);
        }

        public void LogUnauthorizedAccess(string username, string ipAddress, string resource)
        {
            _securityLogger.Warning(
                "Unauthorized access attempt to {Resource} by user {Username} from IP {IpAddress}",
                resource, username, ipAddress);
        }

        public void LogSecurityEvent(string eventType, string details, LogLevel level = LogLevel.Information)
        {
            var logEvent = new LogEvent(
                DateTimeOffset.UtcNow,
                level.ToSerilogLevel(),
                null,
                MessageTemplate.Empty,
                new[]
                {
                    new LogEventProperty("EventType", new ScalarValue(eventType)),
                    new LogEventProperty("Details", new ScalarValue(details))
                });

            _securityLogger.Write(logEvent);
        }
    }

    public static class LogLevelExtensions
    {
        public static LogEventLevel ToSerilogLevel(this LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => LogEventLevel.Verbose,
                LogLevel.Debug => LogEventLevel.Debug,
                LogLevel.Information => LogEventLevel.Information,
                LogLevel.Warning => LogEventLevel.Warning,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Critical => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }
    }
} 