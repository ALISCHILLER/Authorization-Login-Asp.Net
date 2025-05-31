using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Domain.Enums;

namespace Authorization_Login_Asp.Net.Domain.Entities
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
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// گروه دسترسی (مثلاً "UserManagement", "ContentManagement")
        /// برای دسته‌بندی و سازماندهی بهتر دسترسی‌ها
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Group { get; set; }

        /// <summary>
        /// نوع دسترسی (خواندن، نوشتن، حذف و ...)
        /// </summary>
        [Required]
        public PermissionType Type { get; set; }

        /// <summary>
        /// منبع یا سامانه‌ای که این پرمیشن به آن تعلق دارد
        /// </summary>
        [MaxLength(200)]
        public string Resource { get; set; }

        /// <summary>
        /// عملیات یا اکشن پرمیشن
        /// </summary>
        [MaxLength(50)]
        public string Action { get; set; }

        /// <summary>
        /// توضیح کامل‌تر یا اختیاری درباره عملکرد این پرمیشن
        /// </summary>
        [MaxLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// تاریخ ایجاد پرمیشن
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی پرمیشن
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// وضعیت فعال یا غیرفعال بودن پرمیشن
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// مجموعه نقش‌هایی که این پرمیشن به آنها تعلق دارد
        /// ارتباط یک‌به‌چند از طریق RolePermission
        /// </summary>
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        /// <summary>
        /// مجموعه نقش‌هایی که این پرمیشن به آنها تعلق دارد
        /// ارتباط یک‌به‌چند از طریق Role
        /// </summary>
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}
