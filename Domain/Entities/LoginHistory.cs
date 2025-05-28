using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization_Login_Asp.Net.Domain.Entities
{
    /// <summary>
    /// مدل تاریخچه ورود کاربر
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
        /// تاریخ ورود
        /// </summary>
        public DateTime LoginAt { get; set; }

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
        /// مرورگر
        /// </summary>
        [MaxLength(50)]
        public string Browser { get; set; }

        /// <summary>
        /// سیستم عامل
        /// </summary>
        [MaxLength(50)]
        public string OperatingSystem { get; set; }

        /// <summary>
        /// موقعیت مکانی
        /// </summary>
        [MaxLength(100)]
        public string Location { get; set; }

        /// <summary>
        /// وضعیت موفقیت
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// دلیل شکست (در صورت ناموفق بودن)
        /// </summary>
        [MaxLength(200)]
        public string FailureReason { get; set; }

        /// <summary>
        /// کاربر
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
    }
} 