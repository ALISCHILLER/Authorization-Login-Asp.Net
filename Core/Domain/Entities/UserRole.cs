using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Exceptions;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// مدل ارتباط بین کاربر و نقش
    /// این کلاس نماینده جدول UserRoles در دیتابیس است
    /// </summary>
    public class UserRole : BaseEntity
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required]
        public Guid UserId { get; private set; }

        /// <summary>
        /// شناسه نقش
        /// </summary>
        [Required]
        public Guid RoleId { get; private set; }

        /// <summary>
        /// تاریخ انقضا
        /// </summary>
        public DateTime? ExpiresAt { get; private set; }

        /// <summary>
        /// آیا این نقش اصلی کاربر است
        /// </summary>
        public bool IsPrimary { get; private set; }

        /// <summary>
        /// کاربر
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; private set; }

        /// <summary>
        /// نقش
        /// </summary>
        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; private set; }

        /// <summary>
        /// سازنده پیش‌فرض برای EF Core
        /// </summary>
        protected UserRole() { }

        /// <summary>
        /// ایجاد ارتباط جدید بین کاربر و نقش
        /// </summary>
        public static UserRole Create(Guid userId, Guid roleId, bool isPrimary = false, DateTime? expiresAt = null)
        {
            ValidateIds(userId, roleId);
            ValidateExpiration(expiresAt);

            return new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId,
                IsPrimary = isPrimary,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// به‌روزرسانی نقش
        /// </summary>
        public void UpdateRole(Guid newRoleId)
        {
            ValidateIds(UserId, newRoleId);
            
            if (IsPrimary)
                throw new DomainException("نقش اصلی قابل تغییر نیست");

            RoleId = newRoleId;
            Update();
        }

        /// <summary>
        /// تمدید تاریخ انقضا
        /// </summary>
        public void ExtendExpiration(DateTime newExpirationDate)
        {
            ValidateExpiration(newExpirationDate);
            
            if (ExpiresAt.HasValue && newExpirationDate <= ExpiresAt.Value)
                throw new DomainException("تاریخ انقضای جدید باید بزرگتر از تاریخ فعلی باشد");

            ExpiresAt = newExpirationDate;
            Update();
        }

        /// <summary>
        /// بررسی معتبر بودن نقش
        /// </summary>
        public bool IsValid()
        {
            return !ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow;
        }

        /// <summary>
        /// تغییر وضعیت نقش اصلی
        /// </summary>
        public void SetPrimary(bool isPrimary)
        {
            IsPrimary = isPrimary;
            Update();
        }

        private static void ValidateIds(Guid userId, Guid roleId)
        {
            if (userId == Guid.Empty)
                throw new DomainException("شناسه کاربر نمی‌تواند خالی باشد");
            if (roleId == Guid.Empty)
                throw new DomainException("شناسه نقش نمی‌تواند خالی باشد");
        }

        private static void ValidateExpiration(DateTime? expirationDate)
        {
            if (expirationDate.HasValue && expirationDate.Value <= DateTime.UtcNow)
                throw new DomainException("تاریخ انقضا باید در آینده باشد");
        }
    }
}