using System;
using System.Collections.Generic;
using System.Linq;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Exceptions;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// کلاس مقدار دسترسی‌های نقش
    /// </summary>
    public class RolePermissions : ValueObject
    {
        private readonly HashSet<string> _permissions;

        /// <summary>
        /// دسترسی‌ها
        /// </summary>
        public IReadOnlyCollection<string> Permissions => _permissions.ToList().AsReadOnly();

        protected RolePermissions()
        {
            _permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public static RolePermissions Create()
        {
            return new RolePermissions();
        }

        public void AddPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                throw new ArgumentException("نام دسترسی نمی‌تواند خالی باشد", nameof(permission));

            _permissions.Add(permission.Trim());
        }

        public void RemovePermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                throw new ArgumentException("نام دسترسی نمی‌تواند خالی باشد", nameof(permission));

            _permissions.Remove(permission.Trim());
        }

        public bool HasPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                throw new ArgumentException("نام دسترسی نمی‌تواند خالی باشد", nameof(permission));

            return _permissions.Contains(permission.Trim());
        }

        public void Clear()
        {
            _permissions.Clear();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            return _permissions.OrderBy(x => x);
        }
    }
} 