using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس اعلان‌ها
    /// این اینترفیس تمام عملیات مربوط به ارسال اعلان‌ها را تعریف می‌کند
    /// </summary>
    public interface INotificationService
    {
        #region Email Notifications

        /// <summary>
        /// ارسال ایمیل تأیید
        /// </summary>
        Task SendVerificationEmailAsync(string email, Guid userId);

        /// <summary>
        /// ارسال ایمیل بازنشانی رمز عبور
        /// </summary>
        Task SendPasswordResetEmailAsync(string email, string resetCode);

        /// <summary>
        /// ارسال ایمیل کد دو مرحله‌ای
        /// </summary>
        Task SendTwoFactorCodeEmailAsync(string email, string code);

        #endregion

        #region SMS Notifications

        /// <summary>
        /// ارسال پیامک تأیید
        /// </summary>
        Task SendVerificationSmsAsync(string phoneNumber, string code);

        /// <summary>
        /// ارسال پیامک کد دو مرحله‌ای
        /// </summary>
        Task SendTwoFactorCodeSmsAsync(string phoneNumber, string code);

        #endregion

        #region System Notifications

        /// <summary>
        /// ارسال اعلان سیستمی
        /// </summary>
        Task SendSystemAlertAsync(string title, string message, AlertSeverity severity);

        /// <summary>
        /// ارسال اعلان امنیتی
        /// </summary>
        Task SendSecurityAlertAsync(string title, string message, AlertSeverity severity);

        #endregion

        Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(int count = 10);
        Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(NotificationFilter filter);
        Task<NotificationResponse> CreateNotificationAsync(NotificationRequest request);
        Task MarkAsReadAsync(string id);
        Task DeleteNotificationAsync(string id);
    }
}