using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.Base
{
    /// <summary>
    /// کلاس پایه برای رویدادهای تغییر وضعیت
    /// این کلاس شامل ویژگی‌های مشترک تمام رویدادهای تغییر وضعیت است
    /// </summary>
    /// <typeparam name="TStatus">نوع وضعیت</typeparam>
    public abstract class StatusChangeEvent<TStatus> : UserEvent
    {
        /// <summary>
        /// وضعیت قبلی
        /// </summary>
        public TStatus OldStatus { get; }

        /// <summary>
        /// وضعیت جدید
        /// </summary>
        public TStatus NewStatus { get; }

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        protected StatusChangeEvent(
            Guid userId,
            TStatus oldStatus,
            TStatus newStatus,
            string ipAddress = null,
            string userAgent = null,
            string reason = null)
            : base(userId, ipAddress, userAgent, reason)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }

        /// <summary>
        /// تبدیل رویداد به رشته
        /// </summary>
        public override string ToString()
        {
            return $"{base.ToString()} - Status changed from {OldStatus} to {NewStatus}";
        }
    }
} 