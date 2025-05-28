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
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        /// <summary>
        /// ثبت تلاش ورود ناموفق
        /// </summary>
        public async Task LogFailedLoginAttemptAsync(string username, string ipAddress, string userAgent, string reason)
        {
            await Task.Run(() => _securityLogger.Warning(
                "Failed login attempt for user {Username} from IP {IpAddress} using {UserAgent}. Reason: {Reason}",
                username, ipAddress, userAgent, reason));
        }

        /// <summary>
        /// ثبت تغییر رمز عبور
        /// </summary>
        public async Task LogPasswordChangeAsync(Guid userId, string ipAddress)
        {
            await Task.Run(() => _securityLogger.Information(
                "Password changed for user {UserId} from IP {IpAddress}",
                userId, ipAddress));
        }

        /// <summary>
        /// ثبت تغییر ایمیل
        /// </summary>
        public async Task LogEmailChangeAsync(Guid userId, string oldEmail, string newEmail, string ipAddress)
        {
            await Task.Run(() => _securityLogger.Information(
                "Email changed for user {UserId} from {OldEmail} to {NewEmail} from IP {IpAddress}",
                userId, oldEmail, newEmail, ipAddress));
        }

        /// <summary>
        /// ثبت تغییر شماره تلفن
        /// </summary>
        public async Task LogPhoneChangeAsync(Guid userId, string oldPhone, string newPhone, string ipAddress)
        {
            await Task.Run(() => _securityLogger.Information(
                "Phone number changed for user {UserId} from {OldPhone} to {NewPhone} from IP {IpAddress}",
                userId, oldPhone, newPhone, ipAddress));
        }

        /// <summary>
        /// ثبت فعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        public async Task LogTwoFactorActivationAsync(Guid userId, string ipAddress, string method)
        {
            await Task.Run(() => _securityLogger.Information(
                "Two-factor authentication activated for user {UserId} using method {Method} from IP {IpAddress}",
                userId, method, ipAddress));
        }

        /// <summary>
        /// ثبت غیرفعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        public async Task LogTwoFactorDeactivationAsync(Guid userId, string ipAddress)
        {
            await Task.Run(() => _securityLogger.Information(
                "Two-factor authentication deactivated for user {UserId} from IP {IpAddress}",
                userId, ipAddress));
        }

        /// <summary>
        /// ثبت تلاش دسترسی غیرمجاز
        /// </summary>
        public async Task LogUnauthorizedAccessAttemptAsync(Guid userId, string ipAddress, string resource, string action)
        {
            await Task.Run(() => _securityLogger.Warning(
                "Unauthorized access attempt to {Resource} with action {Action} by user {UserId} from IP {IpAddress}",
                resource, action, userId, ipAddress));
        }

        /// <summary>
        /// ثبت رویداد امنیتی عمومی
        /// </summary>
        private async Task LogSecurityEventAsync(string eventType, string details, LogEventLevel level = LogEventLevel.Information)
        {
            await Task.Run(() => _securityLogger.Write(level,
                "Security event: {EventType}. Details: {Details}",
                eventType, details));
        }
    }

    /// <summary>
    /// متدهای کمکی برای تبدیل سطوح لاگ
    /// </summary>
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