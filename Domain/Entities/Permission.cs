using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        /// توضیح کامل‌تر یا اختیاری درباره عملکرد این پرمیشن
        /// </summary>
        [MaxLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// نوع یا دسته‌بندی پرمیشن (مثلاً مدیریت کاربران، گزارش‌ها و ...)
        /// </summary>
        [Required]
        public Enums.PermissionType Type { get; set; }

        /// <summary>
        /// مجموعه نقش‌هایی که این پرمیشن به آنها تعلق دارد
        /// ارتباط یک‌به‌چند از طریق RolePermission
        /// </summary>
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
