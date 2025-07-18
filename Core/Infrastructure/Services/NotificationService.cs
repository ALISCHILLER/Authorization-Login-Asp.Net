using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Notifications;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Microsoft.Extensions.Configuration;
using NotificationEntity = Authorization_Login_Asp.Net.Core.Domain.Entities.Notification;
using SystemNotification = Authorization_Login_Asp.Net.Core.Domain.Entities.SystemNotification;
using SecurityNotification = Authorization_Login_Asp.Net.Core.Domain.Entities.SecurityNotification;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Sms;
using System.Threading;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    /// <summary>
    /// سرویس یکپارچه اعلان‌ها
    /// این سرویس تمام عملیات مربوط به ارسال اعلان‌ها (ایمیل، پیامک، اعلان‌های سیستمی) را مدیریت می‌کند
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<NotificationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITracingService _tracingService;
        private readonly ApplicationDbContext _dbContext;

        public NotificationService(
            IEmailService emailService,
            ISmsService smsService,
            ILogger<NotificationService> logger,
            IConfiguration configuration,
            ITracingService tracingService,
            ApplicationDbContext dbContext)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tracingService = tracingService ?? throw new ArgumentNullException(nameof(tracingService));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        #region Email Notifications

        public async Task SendVerificationEmailAsync(string email, Guid userId)
        {
            var verificationUrl = $"{_configuration["Application:BaseUrl"]}/api/auth/verify-email?userId={userId}";
            var subject = "تأیید ایمیل";
            var message = $"سلام، برای تأیید ایمیل خود، روی لینک زیر کلیک کنید: {verificationUrl}";
            await SendEmailAsync(email, subject, message, NotificationPriority.High);
        }

        private async Task SendEmailAsync(string email, string subject, string message, NotificationPriority priority, Dictionary<string, string>? metadata = null)
        {
            try
            {
                var emailRequest = new EmailRequest
                {
                    To = email,
                    Subject = subject,
                    Body = message,
                    IsHtml = true,
                    Headers = metadata ?? new Dictionary<string, string>()
                };
                await _emailService.SendEmailAsync(emailRequest);
                _logger.LogInformation("ایمیل با موفقیت به {Email} ارسال شد", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال ایمیل به {Email}", email);
                throw;
            }
        }

        #endregion

        #region SMS Notifications

        public async Task SendVerificationSmsAsync(string phoneNumber, string code)
        {
            try
            {
                await _smsService.SendVerificationCodeAsync(phoneNumber, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال پیامک تأیید");
                throw;
            }
        }

        #endregion

        #region CRUD Operations

        public async Task<NotificationDto> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
        {
            var notification = new Notification(
                request.UserId,
                request.Title,
                request.Message,
                request.NotificationType,
                request.NotificationPriority,
                request.ActionUrl,
                request.ActionText,
                request.Icon,
                request.Color,
                request.ExpiresAt);
            await _dbContext.Notifications.AddAsync(notification, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return MapToDto(notification);
        }

        public async Task<NotificationDto> UpdateAsync(UpdateNotificationRequest request, CancellationToken cancellationToken = default)
        {
            var notification = await _dbContext.Notifications.FindAsync(new object[] { request.Id }, cancellationToken);
            if (notification == null) throw new Exception("Notification not found");
            notification.IsRead = request.IsRead;
            if (request.IsRead) notification.ReadAt = DateTime.UtcNow;
            _dbContext.Notifications.Update(notification);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return MapToDto(notification);
        }

        public async Task DeleteAsync(DeleteNotificationRequest request, CancellationToken cancellationToken = default)
        {
            var notification = await _dbContext.Notifications.FindAsync(new object[] { request.Id }, cancellationToken);
            if (notification == null) throw new Exception("Notification not found");
            _dbContext.Notifications.Remove(notification);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(GetNotificationsRequest request, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Notifications.Where(n => n.UserId == request.UserId);
            if (!request.IncludeRead) query = query.Where(n => !n.IsRead);
            if (request.FromDate.HasValue) query = query.Where(n => n.CreatedAt >= request.FromDate.Value);
            if (request.ToDate.HasValue) query = query.Where(n => n.CreatedAt <= request.ToDate.Value);
            if (request.NotificationType.HasValue) query = query.Where(n => n.NotificationType == request.NotificationType);
            var notifications = await query.OrderByDescending(n => n.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
            return notifications.Select(MapToDto);
        }

        #endregion

        #region Mapping helper

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                NotificationType = notification.NotificationType,
                NotificationPriority = notification.NotificationPriority,
                ActionUrl = notification.ActionUrl,
                ActionText = notification.ActionText,
                Icon = notification.Icon,
                Color = notification.Color,
                ExpiresAt = notification.ExpiresAt,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt
            };
        }

        #endregion
    }
}