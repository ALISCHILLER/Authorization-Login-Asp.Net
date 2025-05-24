using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization_Login_Asp.Net.Domain.Entities
{
    /// <summary>
    /// مدل توکن رفرش برای احراز هویت و مدیریت امنیت توکن‌ها
    /// </summary>
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// مقدار رشته‌ای توکن
        /// </summary>
        [Required]
        public string Token { get; set; }

        /// <summary>
        /// تاریخ ایجاد توکن
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن
        /// </summary>
        [Required]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// آی‌پی کاربر هنگام ایجاد توکن (برای بررسی امنیتی)
        /// </summary>
        public string CreatedByIp { get; set; }

        /// <summary>
        /// تاریخ لغو توکن
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// آی‌پی کاربر هنگام لغو توکن
        /// </summary>
        public string RevokedByIp { get; set; }

        /// <summary>
        /// دلیل لغو توکن (برای لاگ و بررسی امنیتی)
        /// </summary>
        public string RevocationReason { get; set; }

        /// <summary>
        /// شناسه توکن جایگزین در صورت Token Rotation
        /// </summary>
        public Guid? ReplacedByTokenId { get; set; }

        /// <summary>
        /// ارجاع به توکن جایگزین (Navigation Property)
        /// </summary>
        [ForeignKey(nameof(ReplacedByTokenId))]
        public RefreshToken ReplacedByToken { get; set; }

        /// <summary>
        /// شناسه کاربر صاحب توکن
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر صاحب توکن
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        /// <summary>
        /// آیا توکن منقضی شده است؟
        /// </summary>
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// آیا توکن لغو شده است؟
        /// </summary>
        [NotMapped]
        public bool IsRevoked => RevokedAt != null;

        /// <summary>
        /// آیا توکن معتبر است؟ (نه لغو شده و نه منقضی)
        /// </summary>
        [NotMapped]
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
