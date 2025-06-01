using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// مدل پرمیشن‌ها (دسترسی‌ها)
    /// این کلاس نماینده جدول Permissions در دیتابیس است.
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// کلید اصلی پرمیشن
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// نام یکتا و کوتاه پرمیشن (مثلاً "CanEdit", "CanDelete")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// توضیح کامل‌تر یا اختیاری درباره عملکرد این پرمیشن
        /// </summary>
        [MaxLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// نام سامانه‌ای که این پرمیشن به آن تعلق دارد
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SystemName { get; set; }

        /// <summary>
        /// تاریخ ایجاد پرمیشن
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// وضعیت فعال یا غیرفعال بودن پرمیشن
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// نقش‌هایی که این پرمیشن را دارند
        /// </summary>
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        /// <summary>
        /// نقش‌هایی که این پرمیشن را دارند
        /// </summary>
        public virtual ICollection<Role> Roles => RolePermissions?.Select(rp => rp.Role).ToList() ?? new List<Role>();

        /// <summary>
        /// نقش‌هایی که این پرمیشن را دارند
        /// </summary>
        public virtual Role Role { get; set; }

        /// <summary>
        /// نقش‌هایی که این پرمیشن را دارند
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// افزودن پرمیشن به نقش
        /// </summary>
        public void AddToRole(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (!RolePermissions.Any(rp => rp.RoleId == role.Id))
            {
                RolePermissions.Add(new RolePermission
                {
                    PermissionId = Id,
                    RoleId = role.Id
                });
            }
        }

        /// <summary>
        /// حذف پرمیشن از نقش
        /// </summary>
        public void RemoveFromRole(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var rolePermission = RolePermissions.FirstOrDefault(rp => rp.RoleId == role.Id);
            if (rolePermission != null)
            {
                RolePermissions.Remove(rolePermission);
            }
        }

        /// <summary>
        /// بررسی وجود پرمیشن در نقش
        /// </summary>
        public bool IsInRole(Role role)
        {
            return role != null && RolePermissions.Any(rp => rp.RoleId == role.Id);
        }

        public Permission()
        {
            Id = Guid.NewGuid();
            RolePermissions = new List<RolePermission>();
        }
    }
}
