using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// مدل دسترسی‌ها (Permissions)
    /// این کلاس نماینده جدول Permissions در دیتابیس است و تعریف کننده دسترسی‌های سیستم است
    /// </summary>
    public class Permission : BaseEntity
    {
        /// <summary>
        /// کلید اصلی پرمیشن
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// نام یکتا و کوتاه دسترسی (مثلاً "CanEdit", "CanDelete")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; private set; }

        /// <summary>
        /// توضیح کامل‌تر یا اختیاری درباره عملکرد این دسترسی
        /// </summary>
        [MaxLength(200)]
        public string Description { get; private set; }

        /// <summary>
        /// نام سامانه‌ای که این دسترسی به آن تعلق دارد
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SystemName { get; private set; }

        /// <summary>
        /// تاریخ ایجاد پرمیشن
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// وضعیت فعال یا غیرفعال بودن دسترسی
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// ارتباط‌های این دسترسی با نقش‌ها
        /// </summary>
        private readonly List<RolePermission> _rolePermissions = new();
        public virtual IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

        /// <summary>
        /// نقش‌های مرتبط با این دسترسی
        /// </summary>
        public virtual IReadOnlyCollection<Role> Roles => _rolePermissions.Select(rp => rp.Role).ToList().AsReadOnly();

        /// <summary>
        /// سازنده پیش‌فرض برای EF Core
        /// </summary>
        protected Permission() { }

        /// <summary>
        /// ایجاد دسترسی جدید
        /// </summary>
        /// <param name="name">نام دسترسی</param>
        /// <param name="systemName">نام سامانه</param>
        /// <param name="description">توضیح اختیاری</param>
        /// <returns>نمونه جدید از Permission</returns>
        public static Permission Create(string name, string systemName, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام دسترسی نمی‌تواند خالی باشد", nameof(name));
            if (string.IsNullOrWhiteSpace(systemName))
                throw new ArgumentException("نام سامانه نمی‌تواند خالی باشد", nameof(systemName));

            return new Permission
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                SystemName = systemName.Trim(),
                Description = description?.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات دسترسی
        /// </summary>
        /// <param name="name">نام جدید</param>
        /// <param name="systemName">نام سامانه جدید</param>
        /// <param name="description">توضیح جدید</param>
        public void Update(string name, string systemName, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام دسترسی نمی‌تواند خالی باشد", nameof(name));
            if (string.IsNullOrWhiteSpace(systemName))
                throw new ArgumentException("نام سامانه نمی‌تواند خالی باشد", nameof(systemName));

            Name = name.Trim();
            SystemName = systemName.Trim();
            Description = description?.Trim();
            Update();
        }

        /// <summary>
        /// تغییر وضعیت فعال/غیرفعال دسترسی
        /// </summary>
        /// <param name="isActive">وضعیت جدید</param>
        public void SetActive(bool isActive)
        {
            IsActive = isActive;
            Update();
        }

        /// <summary>
        /// افزودن دسترسی به نقش
        /// </summary>
        /// <param name="role">نقش مورد نظر</param>
        /// <param name="description">توضیح اختیاری برای ارتباط</param>
        public void AddToRole(Role role, string description = null)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (!_rolePermissions.Any(rp => rp.RoleId == role.Id))
            {
                var rolePermission = RolePermission.Create(role.Id, Id, description);
                _rolePermissions.Add(rolePermission);
                Update();
            }
        }

        /// <summary>
        /// حذف دسترسی از نقش
        /// </summary>
        /// <param name="role">نقش مورد نظر</param>
        public void RemoveFromRole(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.RoleId == role.Id);
            if (rolePermission != null)
            {
                _rolePermissions.Remove(rolePermission);
                Update();
            }
        }

        /// <summary>
        /// بررسی وجود دسترسی در نقش
        /// </summary>
        /// <param name="role">نقش مورد نظر</param>
        /// <returns>آیا دسترسی در نقش وجود دارد؟</returns>
        public bool IsInRole(Role role)
        {
            return role != null && _rolePermissions.Any(rp => rp.RoleId == role.Id);
        }
    }
}
