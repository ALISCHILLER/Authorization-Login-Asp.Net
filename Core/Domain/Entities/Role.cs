using System;
using System.Collections.Generic;
using System.Linq;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.ValueObjects;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// مدل نقش کاربری در سیستم
    /// این کلاس نماینده جدول Roles در دیتابیس است و شامل اطلاعات و رفتارهای مرتبط با نقش‌ها است
    /// </summary>
    public class Role : AggregateRoot
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool IsActive { get; private set; } = true;
        public bool IsSystem { get; private set; }
        public RoleType Type { get; private set; }
        public RolePermissions Permissions { get; private set; }

        private readonly List<UserRole> _userRoles = new();
        public virtual IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
        public virtual IReadOnlyCollection<User> Users => _userRoles.Select(ur => ur.User).ToList().AsReadOnly();

        protected Role() { }

        public static Role Create(
            string name,
            string description,
            RoleType type,
            bool isSystem = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام نقش نمی‌تواند خالی باشد", nameof(name));

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Description = description?.Trim(),
                Type = type,
                IsSystem = isSystem,
                Permissions = RolePermissions.Create(),
                CreatedAt = DateTime.UtcNow
            };

            return role;
        }

        public void Update(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام نقش نمی‌تواند خالی باشد", nameof(name));

            if (IsSystem)
                throw new InvalidOperationException("نقش سیستمی قابل ویرایش نیست");

            Name = name.Trim();
            Description = description?.Trim();
            Update();
        }

        public void SetActive(bool isActive)
        {
            if (IsSystem)
                throw new InvalidOperationException("نقش سیستمی قابل غیرفعال‌سازی نیست");

            IsActive = isActive;
            Update();
        }

        public void AssignPermissions(IEnumerable<Permission> permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            if (IsSystem)
                throw new InvalidOperationException("نقش سیستمی قابل ویرایش نیست");

            Permissions.AssignPermissions(permissions);
            Update();
        }

        public void RemovePermissions(IEnumerable<Permission> permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            if (IsSystem)
                throw new InvalidOperationException("نقش سیستمی قابل ویرایش نیست");

            Permissions.RemovePermissions(permissions);
            Update();
        }

        public bool HasPermission(Permission permission)
        {
            return permission != null && Permissions.HasPermission(permission);
        }

        public bool HasPermission(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("نام دسترسی نمی‌تواند خالی باشد", nameof(permissionName));

            return Permissions.HasPermission(permissionName.Trim());
        }

        public void AddUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!_userRoles.Any(ur => ur.UserId == user.Id))
            {
                var userRole = UserRole.Create(user.Id, Id);
                _userRoles.Add(userRole);
                Update();
            }
        }

        public void RemoveUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var userRole = _userRoles.FirstOrDefault(ur => ur.UserId == user.Id);
            if (userRole != null)
            {
                _userRoles.Remove(userRole);
                Update();
            }
        }

        public void ClearUsers()
        {
            if (IsSystem)
                throw new InvalidOperationException("نقش سیستمی قابل ویرایش نیست");

            _userRoles.Clear();
            Update();
        }

        public void AddUsers(IEnumerable<User> users)
        {
            if (users == null)
                throw new ArgumentNullException(nameof(users));

            foreach (var user in users)
            {
                AddUser(user);
            }
        }

        public IEnumerable<User> GetUsers()
        {
            return _userRoles.Select(ur => ur.User).ToList();
        }
    }
}
