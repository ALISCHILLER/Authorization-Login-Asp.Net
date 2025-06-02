using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// مدل ارتباط بین نقش‌ها و دسترسی‌ها
    /// این کلاس نماینده جدول RolePermissions در دیتابیس است
    /// </summary>
    public class RolePermission : BaseEntity
    {
        /// <summary>
        /// شناسه نقش
        /// </summary>
        [Required]
        public Guid RoleId { get; private set; }

        /// <summary>
        /// شناسه دسترسی
        /// </summary>
        [Required]
        public Guid PermissionId { get; private set; }

        /// <summary>
        /// توضیح یا شرح اختیاری برای ارتباط نقش-دسترسی
        /// </summary>
        [MaxLength(200)]
        public string Description { get; private set; }

        /// <summary>
        /// نقش
        /// </summary>
        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; private set; }

        /// <summary>
        /// دسترسی
        /// </summary>
        [ForeignKey(nameof(PermissionId))]
        public virtual Permission Permission { get; private set; }

        /// <summary>
        /// سازنده پیش‌فرض برای EF Core
        /// </summary>
        protected RolePermission() { }

        /// <summary>
        /// ایجاد ارتباط جدید بین نقش و دسترسی
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="description">توضیح اختیاری</param>
        /// <returns>نمونه جدید از RolePermission</returns>
        public static RolePermission Create(Guid roleId, Guid permissionId, string description = null)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException("شناسه نقش نمی‌تواند خالی باشد", nameof(roleId));
            if (permissionId == Guid.Empty)
                throw new ArgumentException("شناسه دسترسی نمی‌تواند خالی باشد", nameof(permissionId));

            return new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                PermissionId = permissionId,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// به‌روزرسانی توضیحات
        /// </summary>
        /// <param name="description">توضیح جدید</param>
        public void UpdateDescription(string description)
        {
            Description = description;
            Update();
        }

        /// <summary>
        /// به‌روزرسانی دسترسی
        /// </summary>
        /// <param name="newPermissionId">شناسه دسترسی جدید</param>
        public void UpdatePermission(Guid newPermissionId)
        {
            if (newPermissionId == Guid.Empty)
                throw new ArgumentException("شناسه دسترسی نمی‌تواند خالی باشد", nameof(newPermissionId));

            PermissionId = newPermissionId;
            Update();
        }
    }
}
