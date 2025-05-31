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
        /// نام نمایشی نقش
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string DisplayName { get; set; }

        /// <summary>
        /// نوع نقش
        /// </summary>
        [Required]
        public RoleType Type { get; set; }

        /// <summary>
        /// آیا نقش سیستمی است
        /// </summary>
        public bool IsSystem { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

        /// <summary>
        /// سازنده
        /// </summary>
        public Role()
        {
            Users = new List<User>();
            RolePermissions = new List<RolePermission>();
            Permissions = new List<Permission>();
            IsSystem = false;
            CreatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// نوع نقش
    /// </summary>
    public enum RoleType
    {
        /// <summary>
        /// نقش سیستمی
        /// </summary>
        System = 1,

        /// <summary>
        /// نقش کاربری
        /// </summary>
        User = 2,

        /// <summary>
        /// نقش مدیریتی
        /// </summary>
        Admin = 3,

        /// <summary>
        /// نقش مهمان
        /// </summary>
        Guest = 4
    }
}
