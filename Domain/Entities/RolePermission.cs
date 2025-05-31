using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization_Login_Asp.Net.Domain.Entities
{
    /// <summary>
    /// مدل ارتباط بین نقش‌ها و پرمیشن‌ها
    /// این کلاس مشخص می‌کند کدام نقش، چه پرمیشن‌هایی دارد.
    /// </summary>
    public class RolePermission
    {
        /// <summary>
        /// کلید اصلی رکورد
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// کلید خارجی نقش (Role)
        /// </summary>
        [Required]
        public Guid RoleId { get; set; }

        /// <summary>
        /// کلید خارجی پرمیشن (Permission)
        /// </summary>
        [Required]
        public Guid PermissionId { get; set; }

        /// <summary>
        /// توضیح یا شرح اختیاری برای نقش-پرمیشن
        /// </summary>
        [MaxLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// زمان ایجاد رکورد
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation Property به Role
        /// </summary>
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }

        /// <summary>
        /// Navigation Property به Permission
        /// </summary>
        [ForeignKey(nameof(PermissionId))]
        public Permission Permission { get; set; }
    }
}
