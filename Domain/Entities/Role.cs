using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Domain.Enums;
using System.Linq;

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
        public virtual ICollection<UserRole> UserRoles { get; private set; }
        public virtual ICollection<RolePermission> RolePermissions { get; private set; }

        public IEnumerable<User> Users => UserRoles?.Select(ur => ur.User);
        public IEnumerable<Permission> Permissions => RolePermissions?.Select(rp => rp.Permission);

        /// <summary>
        /// سازنده
        /// </summary>
        public Role()
        {
            Id = Guid.NewGuid();
            UserRoles = new List<UserRole>();
            RolePermissions = new List<RolePermission>();
        }

        public void AddPermission(Permission permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));

            if (RolePermissions.Any(rp => rp.PermissionId == permission.Id))
                return;

            RolePermissions.Add(new RolePermission
            {
                RoleId = Id,
                PermissionId = permission.Id,
                CreatedAt = DateTime.UtcNow
            });
        }

        public void RemovePermission(Permission permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));

            var rolePermission = RolePermissions.FirstOrDefault(rp => rp.PermissionId == permission.Id);
            if (rolePermission != null)
            {
                RolePermissions.Remove(rolePermission);
            }
        }

        public bool HasPermission(string permissionName)
        {
            return RolePermissions.Any(rp => 
                rp.Permission.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
        }

        public void AddUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (UserRoles.Any(ur => ur.UserId == user.Id))
                return;

            UserRoles.Add(new UserRole
            {
                RoleId = Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            });
        }

        public void RemoveUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var userRole = UserRoles.FirstOrDefault(ur => ur.UserId == user.Id);
            if (userRole != null)
            {
                UserRoles.Remove(userRole);
            }
        }

        public bool HasUser(User user)
        {
            return user != null && UserRoles.Any(ur => ur.UserId == user.Id);
        }
    }
}
