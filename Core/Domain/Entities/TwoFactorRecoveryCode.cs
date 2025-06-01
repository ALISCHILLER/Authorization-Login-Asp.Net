using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// تاریخ استفاده
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// وضعیت فعال بودن کد
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ انقضا
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// وضعیت استفاده شده
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// بررسی معتبر بودن کد
        /// </summary>
        public bool IsValid()
        {
            return IsActive && !UsedAt.HasValue && ExpiresAt > DateTime.UtcNow;
        }

        /// <summary>
        /// استفاده از کد
        /// </summary>
        public void Use()
        {
            UsedAt = DateTime.UtcNow;
            IsActive = false;
        }

        /// <summary>
        /// کاربر
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
    }
}