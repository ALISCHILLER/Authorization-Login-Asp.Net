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
        [MaxLength(50)]
        public string Browser { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        [MaxLength(45)]
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
        /// تاریخ آخرین استفاده
        /// </summary>
        public DateTime LastUsedAt { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// کاربر
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
    }
} 