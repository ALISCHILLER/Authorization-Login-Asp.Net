using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoggingService> _logger;
        private readonly string _logDirectory;
        private readonly JsonSerializerOptions _jsonOptions;

        public LoggingService(IConfiguration configuration, ILogger<LoggingService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            EnsureLogDirectoryExists();
        }

        public void LogSecurityEvent(string eventType, string message, string userId = null, string ipAddress = null)
        {
            try
            {
                var logEntry = new SecurityLogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    EventType = eventType,
                    Message = message,
                    UserId = userId,
                    IpAddress = ipAddress
                };

                var logFile = GetLogFilePath("Security");
                var logContent = JsonSerializer.Serialize(logEntry, _jsonOptions);
                File.AppendAllText(logFile, logContent + Environment.NewLine);

                _logger.LogInformation("Security event logged: {EventType} - {Message}", eventType, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security event: {EventType}", eventType);
            }
        }

        public void LogAuditEvent(string action, string entityType, string entityId, string userId, string details = null)
        {
            try
            {
                var logEntry = new AuditLogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    UserId = userId,
                    Details = details
                };

                var logFile = GetLogFilePath("Audit");
                var logContent = JsonSerializer.Serialize(logEntry, _jsonOptions);
                File.AppendAllText(logFile, logContent + Environment.NewLine);

                _logger.LogInformation("Audit event logged: {Action} on {EntityType} {EntityId}", action, entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log audit event: {Action}", action);
            }
        }

        public void LogPerformanceEvent(string operation, long durationMs, string userId = null, string details = null)
        {
            try
            {
                var logEntry = new PerformanceLogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Operation = operation,
                    DurationMs = durationMs,
                    UserId = userId,
                    Details = details
                };

                var logFile = GetLogFilePath("Performance");
                var logContent = JsonSerializer.Serialize(logEntry, _jsonOptions);
                File.AppendAllText(logFile, logContent + Environment.NewLine);

                _logger.LogInformation("Performance event logged: {Operation} took {DurationMs}ms", operation, durationMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log performance event: {Operation}", operation);
            }
        }

        public async Task LogSecurityEventAsync(string eventType, string message, string userId = null, string ipAddress = null)
        {
            try
            {
                var logEntry = new SecurityLogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    EventType = eventType,
                    Message = message,
                    UserId = userId,
                    IpAddress = ipAddress
                };

                var logFile = GetLogFilePath("Security");
                var logContent = JsonSerializer.Serialize(logEntry, _jsonOptions);
                await File.AppendAllTextAsync(logFile, logContent + Environment.NewLine);

                _logger.LogInformation("Security event logged: {EventType} - {Message}", eventType, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security event: {EventType}", eventType);
            }
        }

        public async Task LogAuditEventAsync(string action, string entityType, string entityId, string userId, string details = null)
        {
            try
            {
                var logEntry = new AuditLogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    UserId = userId,
                    Details = details
                };

                var logFile = GetLogFilePath("Audit");
                var logContent = JsonSerializer.Serialize(logEntry, _jsonOptions);
                await File.AppendAllTextAsync(logFile, logContent + Environment.NewLine);

                _logger.LogInformation("Audit event logged: {Action} on {EntityType} {EntityId}", action, entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log audit event: {Action}", action);
            }
        }

        public async Task LogPerformanceEventAsync(string operation, long durationMs, string userId = null, string details = null)
        {
            try
            {
                var logEntry = new PerformanceLogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Operation = operation,
                    DurationMs = durationMs,
                    UserId = userId,
                    Details = details
                };

                var logFile = GetLogFilePath("Performance");
                var logContent = JsonSerializer.Serialize(logEntry, _jsonOptions);
                await File.AppendAllTextAsync(logFile, logContent + Environment.NewLine);

                _logger.LogInformation("Performance event logged: {Operation} took {DurationMs}ms", operation, durationMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log performance event: {Operation}", operation);
            }
        }

        private string GetLogFilePath(string logType)
        {
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
            return Path.Combine(_logDirectory, $"{logType}_{date}.log");
        }

        private void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }
    }

    public class SecurityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public string IpAddress { get; set; }
    }

    public class AuditLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string UserId { get; set; }
        public string Details { get; set; }
    }

    public class PerformanceLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Operation { get; set; }
        public long DurationMs { get; set; }
        public string UserId { get; set; }
        public string Details { get; set; }
    }
}