using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.DTOs;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public interface INotificationService
    {
        Task SendSystemAlertAsync(string title, string message, AlertSeverity severity);
        Task SendSecurityAlertAsync(string title, string message, AlertSeverity severity);
        Task SendPerformanceAlertAsync(string title, string message, AlertSeverity severity);
        Task SendErrorAlertAsync(string title, string message, AlertSeverity severity);
        Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(int count = 10);
        Task<NotificationResponse> CreateNotificationAsync(NotificationRequest request);
        Task MarkAsReadAsync(string id);
        Task DeleteNotificationAsync(string id);
    }

    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
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

                await _notificationRepository.AddAsync(notification);
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

                await _notificationRepository.AddAsync(notification);
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

                await _notificationRepository.AddAsync(notification);
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

                await _notificationRepository.AddAsync(notification);
                _logger.LogError("Error alert sent: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending error alert");
                throw;
            }
        }

        public async Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(int count = 10)
        {
            try
            {
                var notifications = await _notificationRepository.GetNotificationsAsync(count);
                return notifications.Select(MapToResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                throw;
            }
        }

        public async Task<NotificationResponse> CreateNotificationAsync(NotificationRequest request)
        {
            try
            {
                var notification = new Notification
                {
                    Title = request.Title,
                    Message = request.Message,
                    Type = request.Type,
                    UserId = request.UserId,
                    ExpiryDate = request.ExpiryDate
                };

                var createdNotification = await _notificationRepository.AddAsync(notification);
                return MapToResponse(createdNotification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }

        public async Task MarkAsReadAsync(string id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    throw new KeyNotFoundException($"Notification with id {id} not found");
                }

                notification.MarkAsRead();
                await _notificationRepository.UpdateAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                throw;
            }
        }

        public async Task DeleteNotificationAsync(string id)
        {
            try
            {
                await _notificationRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                throw;
            }
        }

        private static NotificationResponse MapToResponse(Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                UserId = notification.UserId,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ExpiryDate = notification.ExpiryDate
            };
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
        public string UserId { get; set; }
        public DateTime ExpiryDate { get; set; }

        public void MarkAsRead()
        {
            IsRead = true;
        }
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