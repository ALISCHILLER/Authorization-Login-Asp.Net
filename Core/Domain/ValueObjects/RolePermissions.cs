using System;
using System.Collections.Generic;
using System.Linq;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// Value Object برای مدیریت دسترسی‌های یک نقش
    /// این کلاس شامل مجموعه‌ای از دسترسی‌ها و متدهای مدیریت آن‌ها است
    /// </summary>
    public class RolePermissions : ValueObject
    {
        private readonly List<Permission> _permissions = new();
        public virtual IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

        protected RolePermissions() { }

        public static RolePermissions Create()
        {
            return new RolePermissions();
        }

        public void AssignPermissions(IEnumerable<Permission> permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            foreach (var permission in permissions)
            {
                if (permission == null)
                    continue;

                if (!_permissions.Any(p => p.Id == permission.Id))
                {
                    _permissions.Add(permission);
                }
            }
        }

        public void RemovePermissions(IEnumerable<Permission> permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            foreach (var permission in permissions)
            {
                if (permission == null)
                    continue;

                var existingPermission = _permissions.FirstOrDefault(p => p.Id == permission.Id);
                if (existingPermission != null)
                {
                    _permissions.Remove(existingPermission);
                }
            }
        }

        public bool HasPermission(Permission permission)
        {
            return permission != null && _permissions.Any(p => p.Id == permission.Id);
        }

        public bool HasPermission(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("نام دسترسی نمی‌تواند خالی باشد", nameof(permissionName));

            return _permissions.Any(p =>
                p.Name.Equals(permissionName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public void ClearPermissions()
        {
            _permissions.Clear();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            return _permissions.Select(p => p.Id);
        }
    }
} 