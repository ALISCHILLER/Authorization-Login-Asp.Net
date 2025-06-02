using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.Base
{
    /// <summary>
    /// کلاس پایه برای رویدادهای تایید
    /// این کلاس شامل ویژگی‌های مشترک تمام رویدادهای تایید است
    /// </summary>
    public abstract class VerificationEvent : UserEvent
    {
        /// <summary>
        /// شناسه تایید
        /// </summary>
        public string VerificationId { get; }

        /// <summary>
        /// نوع تایید
        /// </summary>
        public string VerificationType { get; }

        /// <summary>
        /// وضعیت قبلی
        /// </summary>
        public bool WasVerified { get; }

        /// <summary>
        /// وضعیت جدید
        /// </summary>
        public bool IsVerified { get; }

        /// <summary>
        /// زمان انقضای تایید
        /// </summary>
        public DateTime? ExpiresAt { get; }

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        protected VerificationEvent(
            Guid userId,
            string verificationId,
            string verificationType,
            bool wasVerified,
            bool isVerified,
            DateTime? expiresAt = null,
            string ipAddress = null,
            string userAgent = null,
            string reason = null)
            : base(userId, ipAddress, userAgent, reason)
        {
            VerificationId = verificationId;
            VerificationType = verificationType;
            WasVerified = wasVerified;
            IsVerified = isVerified;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// تبدیل رویداد به رشته
        /// </summary>
        public override string ToString()
        {
            return $"{base.ToString()} - {VerificationType} verification status changed from {WasVerified} to {IsVerified}";
        }
    }
} 