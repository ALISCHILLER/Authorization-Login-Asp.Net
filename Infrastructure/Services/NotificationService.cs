using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public interface INotificationService
    {
        Task SendSystemAlertAsync(string title, string message, AlertSeverity severity);
        Task SendSecurityAlertAsync(string title, string message, AlertSeverity severity);
        Task SendPerformanceAlertAsync(string title, string message, AlertSeverity severity);
        Task SendErrorAlertAsync(string title, string message, AlertSeverity severity);
        Task<IEnumerable<Notification>> GetNotificationsAsync(int count = 10);
        Task MarkAsReadAsync(string notificationId);
        Task DeleteNotificationAsync(string notificationId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IEmailService _emailService;
        private readonly IMetricsService _metricsService;
        private readonly ConcurrentDictionary<string, Notification> _notifications = new();
        private readonly object _lockObject = new();

        public NotificationService(
            ILogger<NotificationService> logger,
            IEmailService emailService,
            IMetricsService metricsService)
        {
            _logger = logger;
            _emailService = emailService;
            _metricsService = metricsService;
        }

        public async Task SendSystemAlertAsync(string title, string message, AlertSeverity severity)
        {
            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = title,
                    Message = message,
                    Severity = severity,
                    Type = NotificationType.System,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _notifications.TryAdd(notification.Id, notification);
                await SendEmailNotificationAsync(notification);
                _logger.LogInformation("System alert sent: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system alert");
                throw;
            }
        }

        public async Task SendSecurityAlertAsync(string title, string message, AlertSeverity severity)
        {
            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = title,
                    Message = message,
                    Severity = severity,
                    Type = NotificationType.Security,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _notifications.TryAdd(notification.Id, notification);
                await SendEmailNotificationAsync(notification);
                _logger.LogWarning("Security alert sent: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending security alert");
                throw;
            }
        }

        public async Task SendPerformanceAlertAsync(string title, string message, AlertSeverity severity)
        {
            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = title,
                    Message = message,
                    Severity = severity,
                    Type = NotificationType.Performance,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _notifications.TryAdd(notification.Id, notification);
                await SendEmailNotificationAsync(notification);
                _logger.LogWarning("Performance alert sent: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending performance alert");
                throw;
            }
        }

        public async Task SendErrorAlertAsync(string title, string message, AlertSeverity severity)
        {
            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = title,
                    Message = message,
                    Severity = severity,
                    Type = NotificationType.Error,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _notifications.TryAdd(notification.Id, notification);
                await SendEmailNotificationAsync(notification);
                _logger.LogError("Error alert sent: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending error alert");
                throw;
            }
        }

        public async Task<IEnumerable<Notification>> GetNotificationsAsync(int count = 10)
        {
            return _notifications.Values
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToList();
        }

        public async Task MarkAsReadAsync(string notificationId)
        {
            if (_notifications.TryGetValue(notificationId, out var notification))
            {
                notification.IsRead = true;
                _logger.LogInformation("Notification marked as read: {Id}", notificationId);
            }
        }

        public async Task DeleteNotificationAsync(string notificationId)
        {
            if (_notifications.TryRemove(notificationId, out var notification))
            {
                _logger.LogInformation("Notification deleted: {Id}", notificationId);
            }
        }

        private async Task SendEmailNotificationAsync(Notification notification)
        {
            var subject = $"[{notification.Severity}] {notification.Title}";
            var body = $@"
                <h2>{notification.Title}</h2>
                <p>{notification.Message}</p>
                <p><strong>Type:</strong> {notification.Type}</p>
                <p><strong>Severity:</strong> {notification.Severity}</p>
                <p><strong>Time:</strong> {notification.CreatedAt:yyyy-MM-dd HH:mm:ss}</p>";

            // در اینجا می‌توانید آدرس ایمیل ادمین را از تنظیمات بخوانید
            await _emailService.SendEmailAsync("admin@example.com", subject, body);
        }
    }

    public class Notification
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public AlertSeverity Severity { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public enum NotificationType
    {
        System,
        Security,
        Performance,
        Error
    }

    public enum AlertSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
} 