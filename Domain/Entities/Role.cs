using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Domain.Entities
{
    /// <summary>
    /// مدل نقش (Role)
    /// این کلاس نماینده نقش‌های سیستم مانند Admin، User و ... است.
    /// </summary>
    public class Role
    {
        /// <summary>
        /// کلید اصلی نقش
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// نام یکتا برای نقش (مثلاً Admin، User، Manager)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// توضیح یا شرح اختیاری برای نقش
        /// </summary>
        [MaxLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// مجموعه پرمیشن‌هایی که به این نقش اختصاص داده شده‌اند
        /// </summary>
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
