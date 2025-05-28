using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization_Login_Asp.Net.Domain.Entities
{
    /// <summary>
    /// مدل کدهای بازیابی احراز هویت دو مرحله‌ای
    /// </summary>
    public class TwoFactorRecoveryCode
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
        /// کد بازیابی
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ انقضا
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// تاریخ استفاده
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// وضعیت استفاده شده
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// کاربر
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        /// <summary>
        /// بررسی معتبر بودن کد
        /// </summary>
        public bool IsValid()
        {
            return !IsUsed && ExpiresAt > DateTime.UtcNow;
        }
    }
} 