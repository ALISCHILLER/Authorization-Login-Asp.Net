using System;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// موجودیت نشست کاربر
    /// </summary>
    public class UserSession : BaseEntity
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// توکن دسترسی
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// توکن بازنشانی
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// مرورگر کاربر
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// موقعیت جغرافیایی
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// دستگاه
        /// </summary>
        public string Device { get; set; }

        /// <summary>
        /// سیستم عامل
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// زمان شروع نشست
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// آخرین زمان فعالیت
        /// </summary>
        public DateTime LastActivityAt { get; set; }

        /// <summary>
        /// زمان انقضای توکن دسترسی
        /// </summary>
        public DateTime AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// زمان انقضای توکن بازنشانی
        /// </summary>
        public DateTime RefreshTokenExpiresAt { get; set; }

        /// <summary>
        /// زمان پایان نشست
        /// </summary>
        public DateTime? EndedAt { get; set; }

        /// <summary>
        /// دلیل پایان نشست
        /// </summary>
        public string EndReason { get; set; }

        /// <summary>
        /// آیا نشست فعال است؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// کاربر مرتبط
        /// </summary>
        public virtual User User { get; set; }

        protected UserSession() { }

        public UserSession(
            Guid userId,
            string accessToken,
            string refreshToken,
            DateTime accessTokenExpiresAt,
            DateTime refreshTokenExpiresAt,
            string ipAddress,
            string userAgent,
            string location = null,
            string device = null,
            string operatingSystem = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            RefreshTokenExpiresAt = refreshTokenExpiresAt;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            Location = location;
            Device = device;
            OperatingSystem = operatingSystem;
            CreatedAt = DateTime.UtcNow;
            LastActivityAt = DateTime.UtcNow;
            IsActive = true;
        }

        /// <summary>
        /// به‌روزرسانی زمان آخرین فعالیت
        /// </summary>
        public void UpdateLastActivity()
        {
            LastActivityAt = DateTime.UtcNow;
        }

        /// <summary>
        /// پایان دادن به نشست
        /// </summary>
        public void End(string reason = null)
        {
            if (!IsActive)
                throw new InvalidOperationException("این نشست قبلاً پایان یافته است");

            IsActive = false;
            EndedAt = DateTime.UtcNow;
            EndReason = reason;
        }

        /// <summary>
        /// به‌روزرسانی توکن‌ها
        /// </summary>
        public void UpdateTokens(
            string accessToken,
            string refreshToken,
            DateTime accessTokenExpiresAt,
            DateTime refreshTokenExpiresAt)
        {
            if (!IsActive)
                throw new InvalidOperationException("این نشست فعال نیست");

            AccessToken = accessToken;
            RefreshToken = refreshToken;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            RefreshTokenExpiresAt = refreshTokenExpiresAt;
            LastActivityAt = DateTime.UtcNow;
        }
    }
}