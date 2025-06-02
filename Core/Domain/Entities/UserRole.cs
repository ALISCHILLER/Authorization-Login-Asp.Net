using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Authorization_Login_Asp.Net.Core.Domain.Common;

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
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleId">شناسه نقش</param>
        /// <returns>نمونه جدید از UserRole</returns>
        public static UserRole Create(Guid userId, Guid roleId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("شناسه کاربر نمی‌تواند خالی باشد", nameof(userId));
            if (roleId == Guid.Empty)
                throw new ArgumentException("شناسه نقش نمی‌تواند خالی باشد", nameof(roleId));

            return new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// به‌روزرسانی نقش
        /// </summary>
        /// <param name="newRoleId">شناسه نقش جدید</param>
        public void UpdateRole(Guid newRoleId)
        {
            if (newRoleId == Guid.Empty)
                throw new ArgumentException("شناسه نقش نمی‌تواند خالی باشد", nameof(newRoleId));

            RoleId = newRoleId;
            Update();
        }
    }
}