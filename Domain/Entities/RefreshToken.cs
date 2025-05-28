using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization_Login_Asp.Net.Domain.Entities
{
    /// <summary>
    /// مدل توکن رفرش
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// کلید اصلی
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// توکن
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Token { get; set; }

        /// <summary>
        /// تاریخ انقضا
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ استفاده
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// تاریخ باطل شدن
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// دلیل باطل شدن
        /// </summary>
        [MaxLength(200)]
        public string RevokedReason { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        [MaxLength(45)]
        public string IpAddress { get; set; }

        /// <summary>
        /// اطلاعات دستگاه
        /// </summary>
        [MaxLength(200)]
        public string DeviceInfo { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// کاربر
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        /// <summary>
        /// بررسی معتبر بودن توکن
        /// </summary>
        public bool IsValid()
        {
            return IsActive && !RevokedAt.HasValue && ExpiresAt > DateTime.UtcNow;
        }

        /// <summary>
        /// باطل کردن توکن
        /// </summary>
        /// <param name="reason">دلیل باطل شدن</param>
        public void Revoke(string reason = null)
        {
            RevokedAt = DateTime.UtcNow;
            RevokedReason = reason;
            IsActive = false;
        }
    }
}
