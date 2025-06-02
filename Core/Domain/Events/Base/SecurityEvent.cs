using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.Base
{
    /// <summary>
    /// کلاس پایه برای رویدادهای امنیتی
    /// این کلاس برای رویدادهایی استفاده می‌شود که با امنیت سیستم و کاربران مرتبط هستند
    /// </summary>
    public abstract class SecurityEvent : UserEvent
    {
        /// <summary>
        /// نوع رویداد امنیتی
        /// </summary>
        public string SecurityEventType { get; }

        /// <summary>
        /// سطح خطر رویداد
        /// </summary>
        public SecurityRiskLevel RiskLevel { get; }

        /// <summary>
        /// آیا رویداد نیاز به بررسی فوری دارد
        /// </summary>
        public bool RequiresImmediateAttention { get; }

        /// <summary>
        /// اطلاعات اضافی امنیتی
        /// </summary>
        public object SecurityDetails { get; }

        /// <summary>
        /// سازنده رویداد امنیتی
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="securityEventType">نوع رویداد امنیتی</param>
        /// <param name="riskLevel">سطح خطر</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="userAgent">اطلاعات مرورگر</param>
        /// <param name="requiresImmediateAttention">نیاز به بررسی فوری</param>
        /// <param name="securityDetails">اطلاعات اضافی امنیتی</param>
        /// <param name="reason">دلیل رویداد</param>
        protected SecurityEvent(
            Guid userId,
            string securityEventType,
            SecurityRiskLevel riskLevel,
            string ipAddress,
            string userAgent,
            bool requiresImmediateAttention = false,
            object securityDetails = null,
            string reason = null)
            : base(userId, ipAddress, userAgent, reason)
        {
            SecurityEventType = securityEventType ?? throw new ArgumentNullException(nameof(securityEventType));
            RiskLevel = riskLevel;
            RequiresImmediateAttention = requiresImmediateAttention;
            SecurityDetails = securityDetails;
        }

        /// <summary>
        /// تبدیل رویداد به رشته
        /// </summary>
        public override string ToString()
        {
            return $"رویداد امنیتی: {SecurityEventType} - سطح خطر: {RiskLevel} - کاربر: {UserId} - زمان: {EventTime}";
        }
    }

    /// <summary>
    /// سطوح خطر امنیتی
    /// </summary>
    public enum SecurityRiskLevel
    {
        /// <summary>
        /// کم - نیاز به بررسی معمولی
        /// </summary>
        Low = 0,

        /// <summary>
        /// متوسط - نیاز به بررسی در اسرع وقت
        /// </summary>
        Medium = 1,

        /// <summary>
        /// بالا - نیاز به بررسی فوری
        /// </summary>
        High = 2,

        /// <summary>
        /// بحرانی - نیاز به اقدام فوری
        /// </summary>
        Critical = 3
    }
} 