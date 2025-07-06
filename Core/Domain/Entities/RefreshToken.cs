using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// مدل توکن رفرش
    /// </summary>
    public class RefreshToken : Authorization_Login_Asp.Net.Core.Domain.Common.IEntity
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
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// تاریخ انقضا
        /// </summary>
        [Required]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// آدرس IP ایجاد کننده توکن
        /// </summary>
        [MaxLength(50)]
        public string CreatedByIp { get; set; } = string.Empty;

        /// <summary>
        /// آدرس IP توکن
        /// </summary>
        [MaxLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// تاریخ باطل شدن
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// آدرس IP باطل کننده توکن
        /// </summary>
        [MaxLength(50)]
        public string RevokedByIp { get; set; } = string.Empty;

        /// <summary>
        /// شناسه توکن جایگزین (برای چرخش توکن)
        /// </summary>
        public Guid? ReplacedByTokenId { get; set; }

        /// <summary>
        /// دلیل باطل شدن
        /// </summary>
        [MaxLength(200)]
        public string ReasonRevoked { get; set; } = string.Empty;

        /// <summary>
        /// وضعیت انقضا
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;

        /// <summary>
        /// وضعیت باطل شدن
        /// </summary>
        public bool IsRevoked => RevokedAt != null;

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool IsActive => !IsRevoked && !IsExpired;

        /// <summary>
        /// کاربر
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// توکن جایگزین (برای چرخش توکن)
        /// </summary>
        [ForeignKey(nameof(ReplacedByTokenId))]
        public virtual RefreshToken ReplacedByToken { get; set; } = null!;

        /// <summary>
        /// دلیل باطل شدن
        /// </summary>
        [MaxLength(200)]
        public string RevokedReason { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; internal set; }

        /// <summary>
        /// بررسی معتبر بودن توکن
        /// </summary>
        public bool IsValid()
        {
            return IsActive && !RevokedAt.HasValue && ExpiryDate > DateTime.UtcNow;
        }

        /// <summary>
        /// باطل کردن توکن
        /// </summary>
        /// <param name="reason">دلیل باطل شدن</param>
        /// <param name="replacedByTokenId">شناسه توکن جایگزین (اختیاری)</param>
        public void Revoke(string? reason = null, Guid? replacedByTokenId = null)
        {
            RevokedAt = DateTime.UtcNow;
            ReasonRevoked = reason ?? string.Empty;
            ReplacedByTokenId = replacedByTokenId;
        }

        public RefreshToken()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        public Guid? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
