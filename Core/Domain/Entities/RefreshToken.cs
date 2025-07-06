using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// مدل توکن رفرش
    /// </summary>
    public class RefreshToken : BaseEntity // Inherit from new BaseEntity
    {
        // Id, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted, DeletedAt, DeletedBy are inherited.

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
        public DateTime ExpiresAt { get; set; } // Renamed from ExpiryDate for consistency, removed duplicate ExpiresAt

        /// <summary>
        /// آدرس IP ایجاد کننده توکن
        /// </summary>
        [MaxLength(50)]
        public string CreatedByIp { get; set; } = string.Empty; // Specific to RefreshToken context

        /// <summary>
        /// آدرس IP توکن (می‌تواند همان CreatedByIp باشد یا برای هر استفاده از توکن، آی‌پی جدیدی ثبت شود)
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
        public string RevokedByIp { get; set; } = string.Empty; // Specific to RefreshToken context

        /// <summary>
        /// شناسه توکن جایگزین (برای چرخش توکن)
        /// </summary>
        public Guid? ReplacedByTokenId { get; set; }

        /// <summary>
        /// دلیل باطل شدن
        /// </summary>
        [MaxLength(200)]
        public string ReasonRevoked { get; set; } = string.Empty; // Kept ReasonRevoked, removed RevokedReason

        /// <summary>
        /// وضعیت انقضا
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// وضعیت باطل شدن
        /// </summary>
        public bool IsRevoked => RevokedAt != null;

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool IsActive => !IsRevoked && !IsExpired && !IsDeleted; // Added !IsDeleted check

        /// <summary>
        /// کاربر
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// توکن جایگزین (برای چرخش توکن)
        /// </summary>
        [ForeignKey(nameof(ReplacedByTokenId))]
        public virtual RefreshToken? ReplacedByToken { get; set; } // Made nullable

        /// <summary>
        /// بررسی معتبر بودن توکن
        /// </summary>
        public bool IsValid()
        {
            return IsActive; // IsActive already checks expiry and revocation
        }

        /// <summary>
        /// باطل کردن توکن
        /// </summary>
        /// <param name="reason">دلیل باطل شدن</param>
        /// <param name="revokedByIpAddress">IP Address of the revoker</param>
        /// <param name="replacedByTokenId">شناسه توکن جایگزین (اختیاری)</param>
        /// <param name="updatedByUserId">User ID of the revoker, if applicable</param>
        public void Revoke(string? reason = null, string? revokedByIpAddress = null, Guid? replacedByTokenId = null, string? updatedByUserId = null)
        {
            if (!IsRevoked) // Prevent multiple revocations
            {
                RevokedAt = DateTime.UtcNow;
                ReasonRevoked = reason ?? string.Empty;
                RevokedByIp = revokedByIpAddress ?? string.Empty;
                ReplacedByTokenId = replacedByTokenId;
                MarkAsUpdated(updatedByUserId); // Mark the entity as updated
            }
        }

        // Constructor no longer needs to set Id or CreatedAt manually
        public RefreshToken() : base()
        {
        }

        // Removed manual audit properties as they are inherited from BaseEntity
        // public Guid? CreatedBy { get; set; }
        // public DateTime? LastModifiedAt { get; set; }
        // public Guid? LastModifiedBy { get; set; }
        // public DateTime? DeletedAt { get; set; }
        // public Guid? DeletedBy { get; set; }
        // public bool IsDeleted { get; set; }
    }
}
