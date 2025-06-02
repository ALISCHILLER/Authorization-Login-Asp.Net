using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.Base
{
    /// <summary>
    /// کلاس پایه برای رویدادهای مرتبط با کاربر
    /// این کلاس شامل ویژگی‌های مشترک تمام رویدادهای کاربری است
    /// </summary>
    public abstract class UserEvent : DomainEvent
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public Guid UserId { get; }

        /// <summary>
        /// زمان وقوع رویداد
        /// </summary>
        public DateTime EventTime { get; }

        /// <summary>
        /// آدرس IP کاربر
        /// </summary>
        public string IpAddress { get; }

        /// <summary>
        /// اطلاعات مرورگر کاربر
        /// </summary>
        public string UserAgent { get; }

        /// <summary>
        /// دلیل وقوع رویداد
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        protected UserEvent(
            Guid userId,
            string ipAddress = null,
            string userAgent = null,
            string reason = null)
        {
            UserId = userId;
            EventTime = DateTime.UtcNow;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            Reason = reason;

            // Set base class properties
            EntityId = userId;
            EntityType = "User";
        }
    }
} 