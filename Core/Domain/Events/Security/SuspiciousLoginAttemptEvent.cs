using System;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.Security
{
    /// <summary>
    /// رویداد تلاش مشکوک ورود
    /// این رویداد زمانی رخ می‌دهد که تلاش ورود با مشخصات مشکوک انجام شود
    /// </summary>
    public class SuspiciousLoginAttemptEvent : SecurityEvent
    {
        /// <summary>
        /// تعداد تلاش‌های ناموفق
        /// </summary>
        public int FailedAttempts { get; }

        /// <summary>
        /// موقعیت مکانی تلاش ورود
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// نوع دستگاه
        /// </summary>
        public string DeviceType { get; }

        /// <summary>
        /// سازنده رویداد تلاش مشکوک ورود
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="userAgent">اطلاعات مرورگر</param>
        /// <param name="failedAttempts">تعداد تلاش‌های ناموفق</param>
        /// <param name="location">موقعیت مکانی</param>
        /// <param name="deviceType">نوع دستگاه</param>
        /// <param name="requiresImmediateAttention">نیاز به بررسی فوری</param>
        /// <param name="reason">دلیل رویداد</param>
        public SuspiciousLoginAttemptEvent(
            Guid userId,
            string ipAddress,
            string userAgent,
            int failedAttempts,
            string location = null,
            string deviceType = null,
            bool requiresImmediateAttention = true,
            string reason = null)
            : base(
                userId,
                "SuspiciousLoginAttempt",
                failedAttempts >= 5 ? SecurityRiskLevel.Critical : SecurityRiskLevel.High,
                ipAddress,
                userAgent,
                requiresImmediateAttention,
                new { FailedAttempts = failedAttempts, Location = location, DeviceType = deviceType },
                reason)
        {
            FailedAttempts = failedAttempts;
            Location = location;
            DeviceType = deviceType;
        }

        /// <summary>
        /// تبدیل رویداد به رشته
        /// </summary>
        public override string ToString()
        {
            return $"تلاش مشکوک ورود - کاربر: {UserId} - تعداد تلاش: {FailedAttempts} - موقعیت: {Location} - دستگاه: {DeviceType} - زمان: {EventTime}";
        }
    }
} 