using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization_Login_Asp.Net.Domain.Entities
{
    /// <summary>
    /// مدل دستگاه‌های متصل کاربر
    /// </summary>
    public class UserDevice
    {
        /// <summary>
        /// کلید اصلی
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// شناسه یکتای دستگاه
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string DeviceId { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// نام دستگاه
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string DeviceName { get; set; }

        /// <summary>
        /// نوع دستگاه
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string DeviceType { get; set; }

        /// <summary>
        /// سیستم عامل
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string OperatingSystem { get; set; }

        /// <summary>
        /// مرورگر
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Browser { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string IpAddress { get; set; }

        /// <summary>
        /// موقعیت مکانی
        /// </summary>
        [MaxLength(100)]
        public string Location { get; set; }

        /// <summary>
        /// توکن دستگاه
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string DeviceToken { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین ورود
        /// </summary>
        [Required]
        public DateTime LastLoginAt { get; set; }

        /// <summary>
        /// تاریخ آخرین استفاده
        /// </summary>
        public DateTime LastUsedAt { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// آیا دستگاه مورد اعتماد است
        /// </summary>
        [Required]
        public bool IsTrusted { get; set; }

        /// <summary>
        /// آیا دستگاه مسدود شده است
        /// </summary>
        [Required]
        public bool IsBlocked { get; set; }

        /// <summary>
        /// دلیل مسدود شدن دستگاه
        /// </summary>
        [MaxLength(200)]
        public string BlockReason { get; set; }

        /// <summary>
        /// کاربر
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        /// <summary>
        /// مسدود کردن دستگاه
        /// </summary>
        /// <param name="reason">دلیل مسدود شدن</param>
        public void Block(string reason)
        {
            IsBlocked = true;
            IsActive = false;
            BlockReason = reason;
            LastUsedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// آزاد کردن دستگاه
        /// </summary>
        public void Unblock()
        {
            IsBlocked = false;
            IsActive = true;
            BlockReason = null;
        }

        /// <summary>
        /// به‌روزرسانی وضعیت دستگاه
        /// </summary>
        public void UpdateLastUsed()
        {
            LastUsedAt = DateTime.UtcNow;
            LastLoginAt = DateTime.UtcNow;
        }

        /// <summary>
        /// تنظیم دستگاه به عنوان مورد اعتماد
        /// </summary>
        public void Trust()
        {
            IsTrusted = true;
            IsActive = true;
        }

        /// <summary>
        /// حذف اعتماد از دستگاه
        /// </summary>
        public void Untrust()
        {
            IsTrusted = false;
        }
    }
} 