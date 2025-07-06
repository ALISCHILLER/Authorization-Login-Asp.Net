using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// مدل تاریخچه ورود کاربران
    /// </summary>
    public class LoginHistory
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
        /// زمان ورود
        /// </summary>
        [Required]
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// آدرس IP کاربر
        /// </summary>
        [MaxLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// اطلاعات مرورگر کاربر
        /// </summary>
        [MaxLength(500)]
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>
        /// نام دستگاه
        /// </summary>
        [MaxLength(100)]
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// نوع دستگاه (موبایل، دسکتاپ و ...)
        /// </summary>
        [MaxLength(50)]
        public string DeviceType { get; set; } = string.Empty;

        /// <summary>
        /// سیستم عامل
        /// </summary>
        [MaxLength(50)]
        public string OperatingSystem { get; set; } = string.Empty;

        /// <summary>
        /// نام مرورگر
        /// </summary>
        [MaxLength(100)]
        public string Browser { get; set; } = string.Empty;

        /// <summary>
        /// نسخه مرورگر
        /// </summary>
        [MaxLength(50)]
        public string BrowserVersion { get; set; } = string.Empty;

        /// <summary>
        /// وضعیت موفقیت‌آمیز بودن ورود
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// دلیل عدم موفقیت در صورت ناموفق بودن
        /// </summary>
        [MaxLength(200)]
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>
        /// موقعیت جغرافیایی (کشور)
        /// </summary>
        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// موقعیت جغرافیایی (شهر)
        /// </summary>
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// موقعیت مکانی (ترکیب کشور و شهر)
        /// </summary>
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// زمان خروج
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// مدت زمان حضور کاربر (به ثانیه)
        /// </summary>
        public int? SessionDuration { get; set; }

        /// <summary>
        /// شناسه دستگاه
        /// </summary>
        [Required]
        public Guid DeviceId { get; set; }

        /// <summary>
        /// کاربر
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// وضعیت حذف
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// ثبت زمان خروج و محاسبه مدت زمان حضور
        /// </summary>
        public void Logout()
        {
            LogoutTime = DateTime.UtcNow;
            if (LoginTime != default)
            {
                SessionDuration = (int)(LogoutTime.Value - LoginTime).TotalSeconds;
            }
        }

        /// <summary>
        /// تنظیم موقعیت مکانی
        /// </summary>
        public void SetLocation(string country, string city)
        {
            Country = country;
            City = city;
            Location = $"{country}, {city}".Trim(',', ' ');
        }
    }
}