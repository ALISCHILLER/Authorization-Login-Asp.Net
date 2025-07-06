using System;
using System.Collections.Generic;
using System.Linq;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.ValueObjects;
using Authorization_Login_Asp.Net.Core.Domain.Exceptions;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// مدل نقش کاربری در سیستم
    /// این کلاس نماینده جدول Roles در دیتابیس است و شامل اطلاعات و رفتارهای مرتبط با نقش‌ها است
    /// </summary>
    public class Role : AggregateRoot
    {
        private const int MaxNameLength = 50;
        private const int MaxDescriptionLength = 200;

        public string? Name { get; private set; }
        public string NormalizedName => Name?.ToUpperInvariant() ?? string.Empty;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;
        public bool IsSystem { get; private set; }
        public RoleType Type { get; private set; }
        public RolePermissions Permissions { get; private set; }
        public DateTime? ExpiresAt { get; private set; }

        private readonly List<UserRole> _userRoles = new();
        public virtual IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
        public virtual IReadOnlyCollection<User> Users => _userRoles.Select(ur => ur.User).ToList().AsReadOnly();

        // Navigation property for EF Core - RolePermissions
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        protected Role() {
            Name = string.Empty;
            Description = string.Empty;
            Permissions = Authorization_Login_Asp.Net.Core.Domain.ValueObjects.RolePermissions.Create();
        }

        public static Role Create(
            string name,
            string description,
            RoleType type,
            bool isSystem = false,
            DateTime? expiresAt = null)
        {
            ValidateName(name);
            ValidateDescription(description);
            ValidateExpiration(expiresAt);

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Description = description?.Trim(),
                Type = type,
                IsSystem = isSystem,
                ExpiresAt = expiresAt,
                Permissions = Authorization_Login_Asp.Net.Core.Domain.ValueObjects.RolePermissions.Create(),
                CreatedAt = DateTime.UtcNow
            };

            return role;
        }

        public void Update(string name, string description, RoleType type)
        {
            if (IsSystem)
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException("نقش سیستمی قابل ویرایش نیست");

            ValidateName(name);
            ValidateDescription(description);

            Name = name.Trim();
            Description = description?.Trim();
            Type = type;
            Update();
        }

        public void Deactivate()
        {
            if (IsSystem)
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException("نقش سیستمی قابل غیرفعال‌سازی نیست");

            IsActive = false;
            Update();
        }

        public void Activate()
        {
            IsActive = true;
            Update();
        }

        public void SetExpiration(DateTime? expirationDate)
        {
            if (IsSystem)
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException("نقش سیستمی قابل محدود کردن نیست");

            ValidateExpiration(expirationDate);
            ExpiresAt = expirationDate;
            Update();
        }

        public bool IsExpired()
        {
            return ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
        }

        public bool IsValid()
        {
            return IsActive && !IsExpired();
        }

        #region Permission Management
        public void AddPermission(string permissionName)
        {
            if (IsSystem)
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException("نقش سیستمی قابل ویرایش نیست");

            if (string.IsNullOrWhiteSpace(permissionName))
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException("نام دسترسی نمی‌تواند خالی باشد");

            Permissions.AddPermission(permissionName.Trim());
            Update();
        }

        public void RemovePermission(string permissionName)
        {
            if (IsSystem)
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException("نقش سیستمی قابل ویرایش نیست");

            if (string.IsNullOrWhiteSpace(permissionName))
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException("نام دسترسی نمی‌تواند خالی باشد");

            Permissions.RemovePermission(permissionName.Trim());
            Update();
        }

        public bool HasPermission(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException("نام دسترسی نمی‌تواند خالی باشد");

            return IsValid() && Permissions.HasPermission(permissionName.Trim());
        }

        public void ClearPermissions()
        {
            if (IsSystem)
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException("نقش سیستمی قابل ویرایش نیست");

            Permissions = Authorization_Login_Asp.Net.Core.Domain.ValueObjects.RolePermissions.Create();
            Update();
        }
        #endregion

        #region User Management
        public void AddUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!IsValid())
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException("نقش غیرفعال یا منقضی شده است");

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
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException("نقش سیستمی قابل ویرایش نیست");

            _userRoles.Clear();
            Update();
        }

        public void AddUsers(IEnumerable<User> users)
        {
            if (users == null)
                throw new ArgumentNullException(nameof(users));

            foreach (var user in users.Where(u => u != null))
            {
                AddUser(user);
            }
        }

        public IEnumerable<User> GetUsers()
        {
            return _userRoles.Select(ur => ur.User).Where(u => u != null).ToList();
        }

        public bool HasUser(User user)
        {
            return user != null && _userRoles.Any(ur => ur.UserId == user.Id);
        }

        public bool HasUser(Guid userId)
        {
            return _userRoles.Any(ur => ur.UserId == userId);
        }
        #endregion

        #region Validation
        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException("نام نقش نمی‌تواند خالی باشد");

            if (name.Length > MaxNameLength)
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException($"نام نقش نمی‌تواند بیشتر از {MaxNameLength} کاراکتر باشد");
        }

        private static void ValidateDescription(string description)
        {
            if (!string.IsNullOrWhiteSpace(description) && description.Length > MaxDescriptionLength)
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException($"توضیحات نقش نمی‌تواند بیشتر از {MaxDescriptionLength} کاراکتر باشد");
        }

        private static void ValidateExpiration(DateTime? expirationDate)
        {
            if (expirationDate.HasValue && expirationDate.Value <= DateTime.UtcNow)
                throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException("تاریخ انقضا باید در آینده باشد");
        }
        #endregion

        // Use explicit namespace for DomainException to resolve ambiguity
        private static void ThrowDomainException(string message)
        {
            throw new Authorization_Login_Asp.Net.Core.Domain.Exceptions.DomainException(message);
        }
    }
}
